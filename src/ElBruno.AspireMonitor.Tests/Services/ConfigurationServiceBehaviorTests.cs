using ElBruno.AspireMonitor.Models;
using ElBruno.AspireMonitor.Services;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace ElBruno.AspireMonitor.Tests.Services;

public class ConfigurationServiceBehaviorTests : IDisposable
{
    private readonly string _root = Path.Combine(AppContext.BaseDirectory, "ConfigurationServiceBehavior", Guid.NewGuid().ToString("N"));
    private readonly string _configPath;

    public ConfigurationServiceBehaviorTests()
    {
        _configPath = Path.Combine(_root, "nested", "config.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public void Constructor_WithMissingFile_CreatesDefaultConfigurationFile()
    {
        var service = new ConfigurationService(_configPath);

        var config = service.LoadConfiguration();

        File.Exists(_configPath).Should().BeTrue();
        config.PollingIntervalMs.Should().Be(5000);
        config.ShowMiniWindowResourceTelemetry.Should().BeTrue();
        config.EnableAspireStateNotifications.Should().BeTrue();
        File.ReadAllText(_configPath).Should().Contain("\"notifyOnStateChange\": true");
        service.GetConfiguration().Should().BeSameAs(config);
    }

    [Fact]
    public void SaveConfiguration_PersistsConfigurationForNextServiceInstance()
    {
        var service = new ConfigurationService(_configPath);
        var config = new ElBruno.AspireMonitor.Models.Configuration
        {
            PollingIntervalMs = 3000,
            CpuThresholdWarning = 60,
            CpuThresholdCritical = 85,
            MemoryThresholdWarning = 65,
            MemoryThresholdCritical = 88,
            StartWithWindows = true,
            ProjectFolder = @"C:\Code\App"
        };

        service.SaveConfiguration(config);
        var reloaded = new ConfigurationService(_configPath).LoadConfiguration();

        reloaded.PollingIntervalMs.Should().Be(3000);
        reloaded.CpuThresholdWarning.Should().Be(60);
        reloaded.CpuThresholdCritical.Should().Be(85);
        reloaded.MemoryThresholdWarning.Should().Be(65);
        reloaded.MemoryThresholdCritical.Should().Be(88);
        reloaded.StartWithWindows.Should().BeTrue();
        reloaded.ProjectFolder.Should().Be(@"C:\Code\App");
    }

    [Fact]
    public void Constructor_WithExistingConfigMissingTelemetryToggle_UsesDefaultTrue()
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
          "MiniWindowResources": "api"
        }
        """);

        var config = new ConfigurationService(_configPath).LoadConfiguration();

        config.ShowMiniWindowResourceTelemetry.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithExistingConfigMissingAspireStateNotifications_UsesDefaultEnabled()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
        File.WriteAllText(_configPath, """
        {
          "AspireEndpoint": "http://localhost:18888",
          "PollingIntervalMs": 5000,
          "CpuThresholdWarning": 70,
          "CpuThresholdCritical": 90,
          "MemoryThresholdWarning": 70,
          "MemoryThresholdCritical": 90
        }
        """);

        var config = new ConfigurationService(_configPath).LoadConfiguration();

        config.EnableAspireStateNotifications.Should().BeTrue("legacy config files should keep state notifications enabled by default");
    }

    [Fact]
    public void NewConfiguration_DefaultsMiniWindowResourceTelemetryToVisible()
    {
        var config = new ElBruno.AspireMonitor.Models.Configuration();

        config.ShowMiniWindowResourceTelemetry.Should().BeTrue();
    }

    [Fact]
    public void NewConfiguration_DefaultsAspireStateNotificationsToEnabled()
    {
        var config = new ElBruno.AspireMonitor.Models.Configuration();

        config.EnableAspireStateNotifications.Should().BeTrue();
    }

    [Fact]
    public void SaveConfiguration_PersistsDisabledAspireStateNotifications()
    {
        var service = new ConfigurationService(_configPath);
        var config = new ElBruno.AspireMonitor.Models.Configuration
        {
            EnableAspireStateNotifications = false
        };

        service.SaveConfiguration(config);
        var reloaded = new ConfigurationService(_configPath).LoadConfiguration();

        reloaded.EnableAspireStateNotifications.Should().BeFalse("a user-disabled notification setting should survive restart");
    }

    [Fact]
    public void Constructor_WithDocumentedNotifyOnStateChangeFalse_DisablesAspireStateNotifications()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
        File.WriteAllText(_configPath, """
        {
          "notifyOnStateChange": false
        }
        """);

        var config = new ConfigurationService(_configPath).LoadConfiguration();

        config.EnableAspireStateNotifications.Should().BeFalse("the documented JSON setting should control state notifications");
    }

    [Fact]
    public void SaveConfiguration_WritesDocumentedNotifyOnStateChangeName()
    {
        var service = new ConfigurationService(_configPath);

        service.SaveConfiguration(new ElBruno.AspireMonitor.Models.Configuration
        {
            EnableAspireStateNotifications = false
        });

        var json = File.ReadAllText(_configPath);
        using var document = JsonDocument.Parse(json);

        document.RootElement.GetProperty("notifyOnStateChange").GetBoolean().Should().BeFalse();
        document.RootElement.TryGetProperty("EnableAspireStateNotifications", out _).Should().BeFalse();
    }

    [Fact]
    public void UpdateHelpers_PersistPollingIntervalThresholdsAndDefaults()
    {
        var service = new ConfigurationService(_configPath);

        service.SetPollingInterval(2500);
        service.SetThresholds(55, 80, 56, 81);
        var updated = new ConfigurationService(_configPath).LoadConfiguration();

        updated.PollingIntervalMs.Should().Be(2500);
        updated.CpuThresholdWarning.Should().Be(55);
        updated.CpuThresholdCritical.Should().Be(80);
        updated.MemoryThresholdWarning.Should().Be(56);
        updated.MemoryThresholdCritical.Should().Be(81);

        service.ResetToDefaults();
        var reset = new ConfigurationService(_configPath).LoadConfiguration();
        reset.PollingIntervalMs.Should().Be(5000);
        reset.CpuThresholdWarning.Should().Be(70);
        reset.CpuThresholdCritical.Should().Be(90);
    }

    [Theory]
    [InlineData(499, 70, 90, 70, 90, "PollingIntervalMs must be between 500 and 60000")]
    [InlineData(500, -1, 90, 70, 90, "CpuThresholdWarning must be between 0 and 100")]
    [InlineData(500, 70, 101, 70, 90, "CpuThresholdCritical must be between 0 and 100")]
    [InlineData(500, 70, 90, -1, 90, "MemoryThresholdWarning must be between 0 and 100")]
    [InlineData(500, 70, 90, 70, 101, "MemoryThresholdCritical must be between 0 and 100")]
    [InlineData(500, 90, 90, 70, 90, "CpuThresholdWarning must be less than CpuThresholdCritical")]
    [InlineData(500, 70, 90, 90, 90, "MemoryThresholdWarning must be less than MemoryThresholdCritical")]
    public void SaveConfiguration_WithInvalidValues_Throws(
        int pollingInterval,
        int cpuWarning,
        int cpuCritical,
        int memoryWarning,
        int memoryCritical,
        string expectedMessage)
    {
        var service = new ConfigurationService(_configPath);
        var config = new ElBruno.AspireMonitor.Models.Configuration
        {
            PollingIntervalMs = pollingInterval,
            CpuThresholdWarning = cpuWarning,
            CpuThresholdCritical = cpuCritical,
            MemoryThresholdWarning = memoryWarning,
            MemoryThresholdCritical = memoryCritical
        };

        Action act = () => service.SaveConfiguration(config);

        act.Should().Throw<InvalidOperationException>().WithMessage(expectedMessage);
    }

    [Fact]
    public void Constructor_WithMalformedJson_FallsBackToDefaults()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
        File.WriteAllText(_configPath, "{ bad json");

        var config = new ConfigurationService(_configPath).LoadConfiguration();

        config.PollingIntervalMs.Should().Be(5000);
        config.CpuThresholdWarning.Should().Be(70);
    }

    [Fact]
    public void Constructor_WithInvalidConfigurationJson_FallsBackToDefaults()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
        File.WriteAllText(_configPath, """
        { "PollingIntervalMs": 100, "CpuThresholdWarning": 95, "CpuThresholdCritical": 90 }
        """);

        var config = new ConfigurationService(_configPath).LoadConfiguration();

        config.PollingIntervalMs.Should().Be(5000);
        config.CpuThresholdWarning.Should().Be(70);
    }
}

