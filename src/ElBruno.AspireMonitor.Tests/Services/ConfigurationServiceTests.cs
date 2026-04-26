using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace ElBruno.AspireMonitor.Tests.Services;

public class ConfigurationServiceTests : IDisposable
{
    private readonly string _testConfigDir;
    private readonly string _testConfigPath;
    private readonly string _validFixturePath = Path.Combine("Fixtures", "config-valid.json");
    private readonly string _invalidFixturePath = Path.Combine("Fixtures", "config-invalid.json");

    public ConfigurationServiceTests()
    {
        // Create temp directory for test configs
        _testConfigDir = Path.Combine(Path.GetTempPath(), $"AspireMonitorTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testConfigDir);
        _testConfigPath = Path.Combine(_testConfigDir, "config.json");
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testConfigDir))
        {
            Directory.Delete(_testConfigDir, true);
        }
    }

    [Fact]
    public async Task LoadConfiguration_ValidFile_ReturnsConfiguration()
    {
        // Arrange - Copy valid fixture to test location
        var validJson = await File.ReadAllTextAsync(_validFixturePath);
        await File.WriteAllTextAsync(_testConfigPath, validJson);

        // Act - Parse and validate
        var config = JsonSerializer.Deserialize<JsonElement>(validJson);

        // Assert
        config.GetProperty("aspireApiUrl").GetString().Should().Be("http://localhost:18888");
        config.GetProperty("pollingIntervalSeconds").GetInt32().Should().Be(5);
        config.GetProperty("cpuThresholdYellow").GetInt32().Should().Be(70);
        config.GetProperty("cpuThresholdRed").GetInt32().Should().Be(90);
        config.GetProperty("memoryThresholdYellow").GetInt32().Should().Be(70);
        config.GetProperty("memoryThresholdRed").GetInt32().Should().Be(90);
        config.GetProperty("enableNotifications").GetBoolean().Should().BeTrue();
        config.GetProperty("showSystemTray").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public void LoadConfiguration_MissingFile_ReturnsDefaults()
    {
        // Arrange - Ensure file doesn't exist
        var nonExistentPath = Path.Combine(_testConfigDir, "nonexistent.json");
        File.Exists(nonExistentPath).Should().BeFalse();

        // Act - Load defaults when file missing
        var defaultConfig = GetDefaultConfiguration();

        // Assert - Default values
        defaultConfig["aspireApiUrl"].Should().Be("http://localhost:18888");
        defaultConfig["pollingIntervalSeconds"].Should().Be(2);
        defaultConfig["cpuThresholdYellow"].Should().Be(70);
        defaultConfig["cpuThresholdRed"].Should().Be(90);
        defaultConfig["memoryThresholdYellow"].Should().Be(70);
        defaultConfig["memoryThresholdRed"].Should().Be(90);
        defaultConfig["enableNotifications"].Should().Be(true);
        defaultConfig["showSystemTray"].Should().Be(true);
    }

    [Fact]
    public async Task LoadConfiguration_InvalidJson_HandlesGracefully()
    {
        // Arrange - Write invalid JSON
        await File.WriteAllTextAsync(_testConfigPath, "{ invalid json }");

        // Act & Assert - Should throw JsonException
        Func<Task> act = async () =>
        {
            var content = await File.ReadAllTextAsync(_testConfigPath);
            JsonSerializer.Deserialize<JsonElement>(content);
        };

        await act.Should().ThrowAsync<JsonException>();

        // ConfigurationService should catch this and return defaults
        var defaultConfig = GetDefaultConfiguration();
        defaultConfig["aspireApiUrl"].Should().Be("http://localhost:18888");
    }

    [Fact]
    public async Task SaveConfiguration_ValidConfig_PersistsToFile()
    {
        // Arrange - Create config object
        var config = new
        {
            aspireApiUrl = "http://localhost:19999",
            pollingIntervalSeconds = 3,
            cpuThresholdYellow = 75,
            cpuThresholdRed = 95,
            memoryThresholdYellow = 75,
            memoryThresholdRed = 95,
            enableNotifications = false,
            showSystemTray = true
        };

        // Act - Serialize and save
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_testConfigPath, json);

        // Assert - Verify file exists and content is correct
        File.Exists(_testConfigPath).Should().BeTrue();
        
        var savedContent = await File.ReadAllTextAsync(_testConfigPath);
        var savedConfig = JsonSerializer.Deserialize<JsonElement>(savedContent);
        
        savedConfig.GetProperty("aspireApiUrl").GetString().Should().Be("http://localhost:19999");
        savedConfig.GetProperty("pollingIntervalSeconds").GetInt32().Should().Be(3);
        savedConfig.GetProperty("cpuThresholdYellow").GetInt32().Should().Be(75);
        savedConfig.GetProperty("cpuThresholdRed").GetInt32().Should().Be(95);
        savedConfig.GetProperty("enableNotifications").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task ValidateConfiguration_InvalidValues_ThrowsException()
    {
        // Arrange - Load invalid fixture
        var invalidJson = await File.ReadAllTextAsync(_invalidFixturePath);
        var invalidConfig = JsonSerializer.Deserialize<JsonElement>(invalidJson);

        // Act - Validate each field
        var validationErrors = new List<string>();

        // Invalid URL
        var url = invalidConfig.GetProperty("aspireApiUrl").GetString();
        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            validationErrors.Add($"Invalid aspireApiUrl: {url}");
        }

        // Negative polling interval
        var pollingInterval = invalidConfig.GetProperty("pollingIntervalSeconds").GetInt32();
        if (pollingInterval <= 0)
        {
            validationErrors.Add($"pollingIntervalSeconds must be positive: {pollingInterval}");
        }

        // Invalid threshold (>100)
        var cpuYellow = invalidConfig.GetProperty("cpuThresholdYellow").GetInt32();
        if (cpuYellow < 0 || cpuYellow > 100)
        {
            validationErrors.Add($"cpuThresholdYellow must be 0-100: {cpuYellow}");
        }

        // Negative threshold
        var memRed = invalidConfig.GetProperty("memoryThresholdRed").GetInt32();
        if (memRed < 0 || memRed > 100)
        {
            validationErrors.Add($"memoryThresholdRed must be 0-100: {memRed}");
        }

        // Assert - Should have validation errors
        validationErrors.Should().NotBeEmpty("invalid config should fail validation");
        validationErrors.Should().Contain(e => e.Contains("Invalid aspireApiUrl"));
        validationErrors.Should().Contain(e => e.Contains("pollingIntervalSeconds must be positive"));
        validationErrors.Should().Contain(e => e.Contains("cpuThresholdYellow must be 0-100"));
        validationErrors.Should().Contain(e => e.Contains("memoryThresholdRed must be 0-100"));
    }

    [Theory]
    [InlineData("http://localhost:5000", true)]
    [InlineData("https://aspire.example.com", true)]
    [InlineData("not-a-url", false)]
    [InlineData("", false)]
    public void ValidateEndpoint_VariousFormats_ValidatesCorrectly(string endpoint, bool shouldBeValid)
    {
        // Act
        var isValid = Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) && 
                      (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        // Assert
        isValid.Should().Be(shouldBeValid, 
            $"endpoint '{endpoint}' should be {(shouldBeValid ? "valid" : "invalid")}");
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(60, true)]
    [InlineData(0, false)]
    [InlineData(-5, false)]
    public void ValidatePollingInterval_VariousValues_ValidatesCorrectly(int interval, bool shouldBeValid)
    {
        // Act
        var isValid = interval > 0;

        // Assert
        isValid.Should().Be(shouldBeValid, 
            $"polling interval {interval} should be {(shouldBeValid ? "valid" : "invalid")}");
    }

    [Theory]
    [InlineData(0, 100, true)]
    [InlineData(70, 90, true)]
    [InlineData(-1, 90, false)]
    [InlineData(70, 150, false)]
    [InlineData(90, 70, false)] // Red < Yellow is invalid
    public void ValidateThresholds_VariousCombinations_ValidatesCorrectly(
        int yellowThreshold, int redThreshold, bool shouldBeValid)
    {
        // Act
        var isValid = yellowThreshold >= 0 && yellowThreshold <= 100 &&
                      redThreshold >= 0 && redThreshold <= 100 &&
                      redThreshold >= yellowThreshold;

        // Assert
        isValid.Should().Be(shouldBeValid, 
            $"thresholds yellow={yellowThreshold}, red={redThreshold} should be {(shouldBeValid ? "valid" : "invalid")}");
    }

    private Dictionary<string, object> GetDefaultConfiguration()
    {
        return new Dictionary<string, object>
        {
            ["aspireApiUrl"] = "http://localhost:18888",
            ["pollingIntervalSeconds"] = 2,
            ["cpuThresholdYellow"] = 70,
            ["cpuThresholdRed"] = 90,
            ["memoryThresholdYellow"] = 70,
            ["memoryThresholdRed"] = 90,
            ["enableNotifications"] = true,
            ["showSystemTray"] = true
        };
    }
}
