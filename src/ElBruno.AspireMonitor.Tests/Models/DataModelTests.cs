using System.Collections.Generic;
using ElBruno.AspireMonitor.Models;
using FluentAssertions;
using Xunit;

namespace ElBruno.AspireMonitor.Tests.Models;

/// <summary>
/// Comprehensive unit tests for ResourceMetrics model (Phase 1 Backend Layer).
/// </summary>
public class ResourceMetricsTests
{
    [Fact]
    public void ResourceMetrics_DefaultConstructor_InitializesToZero()
    {
        // Act
        var metrics = new ResourceMetrics();

        // Assert
        metrics.CpuUsagePercent.Should().Be(0);
        metrics.MemoryUsagePercent.Should().Be(0);
        metrics.DiskUsagePercent.Should().Be(0);
    }

    [Fact]
    public void ResourceMetrics_ParameterizedConstructor_SetsValues()
    {
        // Arrange
        var cpu = 45.5;
        var memory = 60.2;
        var disk = 25.8;

        // Act
        var metrics = new ResourceMetrics(cpu, memory, disk);

        // Assert
        metrics.CpuUsagePercent.Should().Be(cpu);
        metrics.MemoryUsagePercent.Should().BeApproximately(memory, 0.0001);
        metrics.DiskUsagePercent.Should().Be(disk);
    }

    [Fact]
    public void ResourceMetrics_ParameterizedConstructorWithoutDisk_DefaultsToDiskZero()
    {
        // Arrange
        var cpu = 45.5;
        var memory = 60.2;

        // Act
        var metrics = new ResourceMetrics(cpu, memory);

        // Assert
        metrics.CpuUsagePercent.Should().Be(cpu);
        metrics.MemoryUsagePercent.Should().BeApproximately(memory, 0.0001);
        metrics.DiskUsagePercent.Should().Be(0);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(50, 50, 50)]
    [InlineData(100, 100, 100)]
    [InlineData(25.5, 75.3, 10.1)]
    public void ResourceMetrics_VariousValues_StoredCorrectly(double cpu, double memory, double disk)
    {
        // Act
        var metrics = new ResourceMetrics(cpu, memory, disk);

        // Assert
        metrics.CpuUsagePercent.Should().Be(cpu);
        metrics.MemoryUsagePercent.Should().BeApproximately(memory, 0.0001);
        metrics.DiskUsagePercent.Should().Be(disk);
    }

    [Fact]
    public void ResourceMetrics_PropertySetters_UpdateValues()
    {
        // Arrange
        var metrics = new ResourceMetrics();

        // Act
        metrics.CpuUsagePercent = 45.5;
        metrics.MemoryUsage = 60.2;
        metrics.MemoryLimit = 100;
        metrics.DiskUsagePercent = 25.8;

        // Assert
        metrics.CpuUsagePercent.Should().Be(45.5);
        metrics.MemoryUsagePercent.Should().BeApproximately(60.2, 0.0001);
        metrics.DiskUsagePercent.Should().Be(25.8);
    }

    [Fact]
    public void ResourceMetrics_NegativeValues_AreAllowed()
    {
        // Arrange & Act
        var metrics = new ResourceMetrics(-10, -20, -30);

        // Assert
        // Note: Model doesn't validate ranges, validation happens elsewhere
        metrics.CpuUsagePercent.Should().Be(-10);
        metrics.MemoryUsagePercent.Should().Be(-20);
        metrics.DiskUsagePercent.Should().Be(-30);
    }

    [Fact]
    public void ResourceMetrics_OverHundredValues_AreAllowed()
    {
        // Arrange & Act
        var metrics = new ResourceMetrics(150, 200, 300);

        // Assert
        // Note: Model doesn't validate ranges, validation happens elsewhere
        metrics.CpuUsagePercent.Should().Be(150);
        metrics.MemoryUsagePercent.Should().Be(200);
        metrics.DiskUsagePercent.Should().Be(300);
    }
}

/// <summary>
/// Comprehensive unit tests for ResourceStatus enum (Phase 1 Backend Layer).
/// </summary>
public class ResourceStatusTests
{
    [Fact]
    public void ResourceStatus_HasExpectedValues()
    {
        // Assert
        Enum.GetValues(typeof(ResourceStatus)).Length.Should().Be(6);
    }

    [Theory]
    [InlineData(ResourceStatus.Unknown, 0)]
    [InlineData(ResourceStatus.Running, 1)]
    [InlineData(ResourceStatus.Stopped, 2)]
    [InlineData(ResourceStatus.Starting, 3)]
    [InlineData(ResourceStatus.Stopping, 4)]
    public void ResourceStatus_EnumValues_HaveCorrectOrder(ResourceStatus status, int expectedValue)
    {
        // Assert
        ((int)status).Should().Be(expectedValue);
    }

    [Fact]
    public void ResourceStatus_CanConvertToString()
    {
        // Arrange
        var status = ResourceStatus.Running;

        // Act
        var stringValue = status.ToString();

        // Assert
        stringValue.Should().Be("Running");
    }

    [Fact]
    public void ResourceStatus_CanParseFromString()
    {
        // Arrange
        var statusString = "Running";

        // Act
        var parsed = Enum.Parse<ResourceStatus>(statusString);

        // Assert
        parsed.Should().Be(ResourceStatus.Running);
    }

    [Theory]
    [InlineData("Unknown", ResourceStatus.Unknown)]
    [InlineData("Running", ResourceStatus.Running)]
    [InlineData("Stopped", ResourceStatus.Stopped)]
    [InlineData("Starting", ResourceStatus.Starting)]
    [InlineData("Stopping", ResourceStatus.Stopping)]
    public void ResourceStatus_AllValues_ParseCorrectly(string input, ResourceStatus expected)
    {
        // Act
        var parsed = Enum.Parse<ResourceStatus>(input);

        // Assert
        parsed.Should().Be(expected);
    }
}

/// <summary>
/// Comprehensive unit tests for Configuration model (Phase 1 Backend Layer).
/// </summary>
public class AspireConfigurationTests
{
    [Fact]
    public void Configuration_DefaultConstructor_InitializesWithDefaults()
    {
        // Act
        var config = new ElBruno.AspireMonitor.Models.Configuration();

        // Assert
        config.PollingIntervalMs.Should().Be(5000);
        config.CpuThresholdWarning.Should().Be(70);
        config.CpuThresholdCritical.Should().Be(90);
        config.MemoryThresholdWarning.Should().Be(70);
        config.MemoryThresholdCritical.Should().Be(90);
        config.StartWithWindows.Should().BeFalse();
        config.ProjectFolder.Should().BeEmpty();
    }

    [Fact]
    public void Configuration_PropertySetters_UpdateValues()
    {
        // Arrange
        var config = new ElBruno.AspireMonitor.Models.Configuration();

        // Act
        config.PollingIntervalMs = 3000;
        config.CpuThresholdWarning = 60;
        config.CpuThresholdCritical = 85;
        config.MemoryThresholdWarning = 65;
        config.MemoryThresholdCritical = 88;
        config.StartWithWindows = true;
        config.ProjectFolder = @"C:\Projects\MyApp";

        // Assert
        config.PollingIntervalMs.Should().Be(3000);
        config.CpuThresholdWarning.Should().Be(60);
        config.CpuThresholdCritical.Should().Be(85);
        config.MemoryThresholdWarning.Should().Be(65);
        config.MemoryThresholdCritical.Should().Be(88);
        config.StartWithWindows.Should().BeTrue();
        config.ProjectFolder.Should().Be(@"C:\Projects\MyApp");
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(2000)]
    [InlineData(5000)]
    [InlineData(10000)]
    public void Configuration_PollingInterval_AcceptsVariousValues(int intervalMs)
    {
        // Arrange
        var config = new ElBruno.AspireMonitor.Models.Configuration();

        // Act
        config.PollingIntervalMs = intervalMs;

        // Assert
        config.PollingIntervalMs.Should().Be(intervalMs);
    }

    [Fact]
    public void Configuration_ThresholdValues_CanBeCustomized()
    {
        // Arrange
        var config = new ElBruno.AspireMonitor.Models.Configuration();

        // Act
        config.CpuThresholdWarning = 50;
        config.CpuThresholdCritical = 75;
        config.MemoryThresholdWarning = 55;
        config.MemoryThresholdCritical = 80;

        // Assert
        config.CpuThresholdWarning.Should().Be(50);
        config.CpuThresholdCritical.Should().Be(75);
        config.MemoryThresholdWarning.Should().Be(55);
        config.MemoryThresholdCritical.Should().Be(80);
    }
}
