using ElBruno.AspireMonitor.Models;
using FluentAssertions;
using Xunit;

namespace ElBruno.AspireMonitor.Tests.Models;

public class AppConfigurationCoverageTests : IDisposable
{
    private readonly string _root = Path.Combine(AppContext.BaseDirectory, "AppConfigurationCoverage", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public void Validate_DefaultConfiguration_IsValid()
    {
        var config = new AppConfiguration();

        config.Validate();

        config.AspireEndpoint.Should().Be("http://localhost:18888");
        config.PollingIntervalMs.Should().Be(2000);
        config.HttpTimeoutSeconds.Should().Be(5);
        config.MaxRetries.Should().Be(3);
    }

    [Theory]
    [InlineData("", "AspireEndpoint cannot be empty")]
    [InlineData("not-a-url", "AspireEndpoint must be a valid URL")]
    public void Validate_InvalidAspireEndpoint_Throws(string endpoint, string expectedMessage)
    {
        var config = new AppConfiguration { AspireEndpoint = endpoint };

        Action act = config.Validate;

        act.Should().Throw<InvalidOperationException>().WithMessage(expectedMessage);
    }

    [Theory]
    [InlineData(499, "PollingIntervalMs must be between 500 and 60000")]
    [InlineData(60001, "PollingIntervalMs must be between 500 and 60000")]
    [InlineData(-1, "CpuThresholdWarning must be between 0 and 100")]
    [InlineData(101, "CpuThresholdCritical must be between 0 and 100")]
    public void Validate_InvalidNumericValues_Throw(int value, string expectedMessage)
    {
        var config = new AppConfiguration();
        if (expectedMessage.StartsWith("PollingIntervalMs"))
        {
            config.PollingIntervalMs = value;
        }
        else if (expectedMessage.StartsWith("CpuThresholdWarning"))
        {
            config.CpuThresholdWarning = value;
        }
        else
        {
            config.CpuThresholdCritical = value;
        }

        Action act = config.Validate;

        act.Should().Throw<InvalidOperationException>().WithMessage(expectedMessage);
    }

    [Theory]
    [InlineData(-1, 90, "MemoryThresholdWarning must be between 0 and 100")]
    [InlineData(70, 101, "MemoryThresholdCritical must be between 0 and 100")]
    [InlineData(90, 90, "CpuThresholdWarning must be less than CpuThresholdCritical")]
    public void Validate_InvalidThresholdRelationships_Throw(int warning, int critical, string expectedMessage)
    {
        var config = new AppConfiguration
        {
            CpuThresholdWarning = expectedMessage.StartsWith("Cpu") ? warning : 70,
            CpuThresholdCritical = expectedMessage.StartsWith("Cpu") ? critical : 90,
            MemoryThresholdWarning = expectedMessage.StartsWith("MemoryThresholdWarning") ? warning : 70,
            MemoryThresholdCritical = expectedMessage.StartsWith("MemoryThresholdCritical") ? critical : 90
        };

        Action act = config.Validate;

        act.Should().Throw<InvalidOperationException>().WithMessage(expectedMessage);
    }

    [Theory]
    [InlineData(0, 3, "HttpTimeoutSeconds must be between 1 and 30")]
    [InlineData(31, 3, "HttpTimeoutSeconds must be between 1 and 30")]
    [InlineData(5, -1, "MaxRetries must be between 0 and 10")]
    [InlineData(5, 11, "MaxRetries must be between 0 and 10")]
    public void Validate_InvalidTimeoutOrRetries_Throws(int timeoutSeconds, int maxRetries, string expectedMessage)
    {
        var config = new AppConfiguration
        {
            HttpTimeoutSeconds = timeoutSeconds,
            MaxRetries = maxRetries
        };

        Action act = config.Validate;

        act.Should().Throw<InvalidOperationException>().WithMessage(expectedMessage);
    }

    [Fact]
    public void Validate_ProjectFolderRequiresAspireMarkers()
    {
        Directory.CreateDirectory(_root);
        var config = new AppConfiguration { ProjectFolder = _root };

        Action act = config.Validate;

        act.Should().Throw<InvalidOperationException>().WithMessage("*missing aspire.config.json or AppHost.cs*");
    }

    [Theory]
    [InlineData("aspire.config.json")]
    [InlineData("AppHost.cs")]
    public void Validate_ProjectFolderWithAspireMarker_IsValid(string markerFile)
    {
        Directory.CreateDirectory(_root);
        File.WriteAllText(Path.Combine(_root, markerFile), "{}");
        var config = new AppConfiguration { ProjectFolder = _root };

        config.Validate();
    }

    [Theory]
    [InlineData("ftp://example.com/repo")]
    [InlineData("not-a-url")]
    public void Validate_InvalidRepositoryUrl_Throws(string repositoryUrl)
    {
        var config = new AppConfiguration { RepositoryUrl = repositoryUrl };

        Action act = config.Validate;

        act.Should().Throw<InvalidOperationException>().WithMessage("RepositoryUrl must be a valid HTTP or HTTPS URL");
    }

    [Fact]
    public void DetectAspireEndpoint_ReturnsNullForMissingFolder()
    {
        AppConfiguration.DetectAspireEndpoint(Path.Combine(_root, "missing")).Should().BeNull();
        AppConfiguration.DetectAspireEndpoint("").Should().BeNull();
    }

    [Fact]
    public void DetectAspireEndpoint_ReturnsConfiguredUrl()
    {
        Directory.CreateDirectory(_root);
        File.WriteAllText(Path.Combine(_root, "aspire.config.json"), """{ "appHost": { "url": "https://localhost:17777" } }""");

        AppConfiguration.DetectAspireEndpoint(_root).Should().Be("https://localhost:17777");
    }

    [Fact]
    public void DetectAspireEndpoint_ReturnsConfiguredPort()
    {
        Directory.CreateDirectory(_root);
        File.WriteAllText(Path.Combine(_root, "aspire.config.json"), """{ "appHost": { "port": 17777 } }""");

        AppConfiguration.DetectAspireEndpoint(_root).Should().Be("http://localhost:17777");
    }

    [Fact]
    public void DetectAspireEndpoint_ReturnsDefaultWhenConfigHasNoEndpoint()
    {
        Directory.CreateDirectory(_root);
        File.WriteAllText(Path.Combine(_root, "aspire.config.json"), """{ "appHost": { } }""");

        AppConfiguration.DetectAspireEndpoint(_root).Should().Be("http://localhost:18888");
    }

    [Fact]
    public void DetectAspireEndpoint_ReturnsNullForMalformedJson()
    {
        Directory.CreateDirectory(_root);
        File.WriteAllText(Path.Combine(_root, "aspire.config.json"), "{ bad json");

        AppConfiguration.DetectAspireEndpoint(_root).Should().BeNull();
    }
}

public class BackendModelCoverageTests
{
    [Fact]
    public void ResourceCollection_UpdatesHealthFromResourceStatuses()
    {
        var healthy = new ResourceCollection(new List<AspireResource>
        {
            new() { Name = "api", Status = ResourceStatus.Running },
            new() { Name = "worker", Status = ResourceStatus.Stopped }
        });
        var failed = new ResourceCollection(new List<AspireResource>
        {
            new() { Name = "api", Status = ResourceStatus.Failed }
        });
        var empty = new ResourceCollection(new List<AspireResource>());

        healthy.IsHealthy.Should().BeTrue();
        failed.IsHealthy.Should().BeFalse();
        empty.IsHealthy.Should().BeFalse();

        healthy.Resources[0].Status = ResourceStatus.Unknown;
        var before = healthy.LastUpdated;
        healthy.UpdateHealth();

        healthy.IsHealthy.Should().BeFalse();
        healthy.LastUpdated.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void AspireHost_ConstructorsAndProperties_Work()
    {
        var host = new AspireHost("http://localhost:18888", "local")
        {
            Version = "1.0",
            OverallStatus = StatusColor.Green,
            Resources = new List<AspireResource> { new() { Name = "api" } }
        };
        var empty = new AspireHost();

        host.Url.Should().Be("http://localhost:18888");
        host.Name.Should().Be("local");
        host.Version.Should().Be("1.0");
        host.OverallStatus.Should().Be(StatusColor.Green);
        host.Resources.Single().Name.Should().Be("api");
        empty.Resources.Should().BeEmpty();
        empty.OverallStatus.Should().Be(StatusColor.Unknown);
    }

    [Theory]
    [InlineData("Running", true)]
    [InlineData("running", true)]
    [InlineData("Stopped", false)]
    public void AspireInstance_IsRunning_IsCaseInsensitive(string status, bool expected)
    {
        var instance = new AspireInstance(123, 18888, status);
        var empty = new AspireInstance();

        instance.ProcessId.Should().Be(123);
        instance.Port.Should().Be(18888);
        instance.Status.Should().Be(status);
        instance.IsRunning.Should().Be(expected);
        instance.DetectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        empty.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void HealthStatus_Constructors_SetExpectedValues()
    {
        var defaultStatus = new HealthStatus();
        var warning = new HealthStatus(StatusColor.Yellow, "High CPU");

        defaultStatus.Color.Should().Be(StatusColor.Unknown);
        defaultStatus.Message.Should().BeEmpty();
        defaultStatus.Timestamp.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(2));
        warning.Color.Should().Be(StatusColor.Yellow);
        warning.Message.Should().Be("High CPU");
        warning.Timestamp.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(2));
    }
}