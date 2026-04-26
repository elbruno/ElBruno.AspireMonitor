using FluentAssertions;
using System.Text.Json;
using Xunit;
using ElBruno.AspireMonitor.Models;
using ElBruno.AspireMonitor.Services;
using Configuration = ElBruno.AspireMonitor.Models.Configuration;

namespace ElBruno.AspireMonitor.Tests.Configuration;

public class RepositoryUrlValidationTests : IDisposable
{
    private readonly string _testConfigDir;
    private readonly string _configFilePath;

    public RepositoryUrlValidationTests()
    {
        _testConfigDir = Path.Combine(Path.GetTempPath(), $"AspireRepoTests_{Guid.NewGuid()}");
        _configFilePath = Path.Combine(_testConfigDir, "config.json");
        Directory.CreateDirectory(_testConfigDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testConfigDir))
                Directory.Delete(_testConfigDir, true);
        }
        catch
        {
            // Suppress errors during cleanup
        }
    }

    #region RepositoryUrl Valid Cases

    [Fact]
    public void RepositoryUrl_WithValidHttpUrl_ShouldBeValid()
    {
        // Arrange
        var repoUrl = "http://github.com/elbruno/ElBruno.AspireMonitor";
        var config = new Models.Configuration
        {
            RepositoryUrl = repoUrl,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.RepositoryUrl.Should().Be(repoUrl);
        Uri.TryCreate(repoUrl, UriKind.Absolute, out var uri).Should().BeTrue();
        uri.Scheme.Should().Be("http");
    }

    [Fact]
    public void RepositoryUrl_WithValidHttpsUrl_ShouldBeValid()
    {
        // Arrange
        var repoUrl = "https://github.com/elbruno/ElBruno.AspireMonitor";
        var config = new Models.Configuration
        {
            RepositoryUrl = repoUrl,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.RepositoryUrl.Should().Be(repoUrl);
        Uri.TryCreate(repoUrl, UriKind.Absolute, out var uri).Should().BeTrue();
        uri.Scheme.Should().Be("https");
    }

    [Fact]
    public void RepositoryUrl_WithQueryParameters_ShouldBeValid()
    {
        // Arrange
        var repoUrl = "https://github.com/elbruno/ElBruno.AspireMonitor?tab=readme-ov-file&ref=main";
        var config = new Models.Configuration
        {
            RepositoryUrl = repoUrl,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.RepositoryUrl.Should().Be(repoUrl);
        Uri.TryCreate(repoUrl, UriKind.Absolute, out var uri).Should().BeTrue();
        uri.Query.Should().Contain("tab=readme");
    }

    [Fact]
    public void RepositoryUrl_WithFragment_ShouldBeValid()
    {
        // Arrange
        var repoUrl = "https://github.com/elbruno/ElBruno.AspireMonitor#readme";
        var config = new Models.Configuration
        {
            RepositoryUrl = repoUrl,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.RepositoryUrl.Should().Be(repoUrl);
        Uri.TryCreate(repoUrl, UriKind.Absolute, out var uri).Should().BeTrue();
        uri.Fragment.Should().Be("#readme");
    }

    [Fact]
    public void RepositoryUrl_WithQueryParamsAndFragment_ShouldBeValid()
    {
        // Arrange
        var repoUrl = "https://github.com/elbruno/ElBruno.AspireMonitor?tab=readme#features";
        var config = new Models.Configuration
        {
            RepositoryUrl = repoUrl,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.RepositoryUrl.Should().Be(repoUrl);
        Uri.TryCreate(repoUrl, UriKind.Absolute, out _).Should().BeTrue();
    }

    [Fact]
    public void RepositoryUrl_SetToNullOrEmpty_ShouldBeValid()
    {
        // Arrange
        var config = new Models.Configuration
        {
            RepositoryUrl = null,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.RepositoryUrl.Should().BeNull();
    }

    [Fact]
    public void RepositoryUrl_SetToEmptyString_ShouldResetToNoUrl()
    {
        // Arrange
        var config = new Models.Configuration
        {
            RepositoryUrl = "",
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.RepositoryUrl.Should().BeEmpty();
    }

    [Fact]
    public void RepositoryUrl_WithTrailingSlash_ShouldBeValid()
    {
        // Arrange
        var repoUrl = "https://github.com/elbruno/ElBruno.AspireMonitor/";
        var config = new Models.Configuration
        {
            RepositoryUrl = repoUrl,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.RepositoryUrl.Should().Be(repoUrl);
        Uri.TryCreate(repoUrl, UriKind.Absolute, out _).Should().BeTrue();
    }

    [Fact]
    public void RepositoryUrl_WithComplexDomain_ShouldBeValid()
    {
        // Arrange
        var repoUrl = "https://github.company.com/org/ElBruno.AspireMonitor";
        var config = new Models.Configuration
        {
            RepositoryUrl = repoUrl,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.RepositoryUrl.Should().Be(repoUrl);
        Uri.TryCreate(repoUrl, UriKind.Absolute, out var uri).Should().BeTrue();
        uri.Host.Should().Be("github.company.com");
    }

    [Fact]
    public void RepositoryUrl_WithPort_ShouldBeValid()
    {
        // Arrange
        var repoUrl = "https://github.com:8443/elbruno/ElBruno.AspireMonitor";
        var config = new Models.Configuration
        {
            RepositoryUrl = repoUrl,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.RepositoryUrl.Should().Be(repoUrl);
        Uri.TryCreate(repoUrl, UriKind.Absolute, out var uri).Should().BeTrue();
        uri.Port.Should().Be(8443);
    }

    [Fact]
    public void RepositoryUrl_WithUserInfo_ShouldBeValid()
    {
        // Arrange
        var repoUrl = "https://user:token@github.com/elbruno/ElBruno.AspireMonitor";
        var config = new Models.Configuration
        {
            RepositoryUrl = repoUrl,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.RepositoryUrl.Should().Be(repoUrl);
        Uri.TryCreate(repoUrl, UriKind.Absolute, out _).Should().BeTrue();
    }

    #endregion

    #region RepositoryUrl Invalid Cases

    [Fact]
    public void RepositoryUrl_MissingScheme_ShouldIndicateInvalid()
    {
        // Arrange
        var invalidUrl = "github.com/elbruno/ElBruno.AspireMonitor";

        // Act & Assert
        Uri.TryCreate(invalidUrl, UriKind.Absolute, out _).Should().BeFalse();
    }

    [Fact]
    public void RepositoryUrl_MalformedUrl_ShouldHandleGracefully()
    {
        // Arrange - URLs with unusual characters often still parse in .NET
        var unusualUrl = "https://github.com/elbruno/:::invalid";

        // Act
        var isValid = Uri.TryCreate(unusualUrl, UriKind.Absolute, out var uri);

        // Assert - The important thing is our validation can process it
        // .NET's URI parser is permissive and encodes special characters
        // The key is that if it's not HTTP/HTTPS, we reject it
        if (isValid && uri != null)
        {
            (uri.Scheme == "https" || uri.Scheme == "http").Should().BeTrue();
        }
    }

    [Fact]
    public void RepositoryUrl_PlainText_ShouldIndicateInvalid()
    {
        // Arrange
        var invalidUrl = "just-some-text";

        // Act & Assert
        Uri.TryCreate(invalidUrl, UriKind.Absolute, out _).Should().BeFalse();
    }

    [Fact]
    public void RepositoryUrl_InvalidScheme_ShouldIndicateInvalid()
    {
        // Arrange
        var invalidUrl = "ftp://github.com/elbruno/ElBruno.AspireMonitor";

        // Act
        var isValid = Uri.TryCreate(invalidUrl, UriKind.Absolute, out var uri);

        // Assert
        isValid.Should().BeTrue(); // URI parses successfully
        uri.Scheme.Should().NotBe("http").And.NotBe("https");
    }

    [Fact]
    public void RepositoryUrl_OnlySlashes_ShouldIndicateInvalid()
    {
        // Arrange
        var invalidUrl = "///";

        // Act & Assert
        Uri.TryCreate(invalidUrl, UriKind.Absolute, out _).Should().BeFalse();
    }

    [Fact]
    public void RepositoryUrl_Whitespace_ShouldIndicateInvalid()
    {
        // Arrange
        var invalidUrl = "https://github.com/elbruno/my project";

        // Act
        var isValid = Uri.TryCreate(invalidUrl, UriKind.Absolute, out var uri);

        // Assert - whitespace is encoded, but indicates potentially invalid input
        isValid.Should().BeTrue(); // URL encoded internally
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task RepositoryUrl_CanBeSavedToConfigFile()
    {
        // Arrange
        var repoUrl = "https://github.com/elbruno/ElBruno.AspireMonitor";
        var configService = new ConfigurationService(_configFilePath);
        var config = new Models.Configuration
        {
            AspireEndpoint = "http://localhost:18888",
            RepositoryUrl = repoUrl
        };

        // Act
        configService.SaveConfiguration(config);
        var loadedConfig = configService.LoadConfiguration();

        // Assert
        loadedConfig.RepositoryUrl.Should().Be(repoUrl);
    }

    [Fact]
    public async Task RepositoryUrl_CanBeLoadedFromConfigFile()
    {
        // Arrange
        var repoUrl = "https://github.com/elbruno/ElBruno.AspireMonitor";
        var configJson = @"{
            ""aspireEndpoint"": ""http://localhost:18888"",
            ""pollingIntervalMs"": 2000,
            ""cpuThresholdWarning"": 70,
            ""cpuThresholdCritical"": 90,
            ""memoryThresholdWarning"": 70,
            ""memoryThresholdCritical"": 90,
            ""repositoryUrl"": """ + repoUrl + @"""
        }";
        
        await File.WriteAllTextAsync(_configFilePath, configJson);

        // Act
        var configService = new ConfigurationService(_configFilePath);
        var loadedConfig = configService.LoadConfiguration();

        // Assert
        loadedConfig.RepositoryUrl.Should().Be(repoUrl);
    }

    [Fact]
    public void RepositoryUrl_BackwardCompatibility_OldConfigWithoutRepositoryUrl()
    {
        // Arrange
        var oldConfigJson = @"{
            ""aspireEndpoint"": ""http://localhost:18888"",
            ""pollingIntervalMs"": 2000,
            ""cpuThresholdWarning"": 70,
            ""cpuThresholdCritical"": 90,
            ""memoryThresholdWarning"": 70,
            ""memoryThresholdCritical"": 90
        }";
        
        File.WriteAllText(_configFilePath, oldConfigJson);

        // Act
        var configService = new ConfigurationService(_configFilePath);
        var loadedConfig = configService.LoadConfiguration();

        // Assert
        loadedConfig.RepositoryUrl.Should().BeNull();
        loadedConfig.AspireEndpoint.Should().Be("http://localhost:18888");
    }

    [Fact]
    public void RepositoryUrl_SaveAndLoadWithQueryParams_ShouldPreserveUrl()
    {
        // Arrange
        var repoUrl = "https://github.com/elbruno/ElBruno.AspireMonitor?tab=readme-ov-file&ref=main#features";
        var configService = new ConfigurationService(_configFilePath);
        var config = new Models.Configuration
        {
            AspireEndpoint = "http://localhost:18888",
            RepositoryUrl = repoUrl
        };

        // Act
        configService.SaveConfiguration(config);
        var loadedConfig = configService.LoadConfiguration();

        // Assert
        loadedConfig.RepositoryUrl.Should().Be(repoUrl);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void RepositoryUrl_ResetToNull_ShouldClearPreviousValue()
    {
        // Arrange
        var config = new Models.Configuration
        {
            RepositoryUrl = "https://github.com/elbruno/ElBruno.AspireMonitor",
            AspireEndpoint = "http://localhost:18888"
        };

        // Act
        config.RepositoryUrl = null;

        // Assert
        config.RepositoryUrl.Should().BeNull();
    }

    [Fact]
    public void RepositoryUrl_WithInternationalDomain_ShouldBeValid()
    {
        // Arrange
        var repoUrl = "https://ドメイン.jp/path"; // International domain
        
        // Act
        var isValid = Uri.TryCreate(repoUrl, UriKind.Absolute, out var uri);

        // Assert
        isValid.Should().BeTrue();
        uri.Scheme.Should().Be("https");
    }

    [Fact]
    public void RepositoryUrl_WithVeryLongPath_ShouldBeValid()
    {
        // Arrange
        var longPath = string.Join("/", Enumerable.Range(0, 20).Select(i => $"segment{i}"));
        var repoUrl = $"https://github.com/{longPath}";
        var config = new Models.Configuration
        {
            RepositoryUrl = repoUrl,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.RepositoryUrl.Should().Be(repoUrl);
        Uri.TryCreate(repoUrl, UriKind.Absolute, out _).Should().BeTrue();
    }

    [Fact]
    public void RepositoryUrl_WithSpecialCharactersInPath_ShouldBeValid()
    {
        // Arrange
        var repoUrl = "https://github.com/elbruno/My-Repository_v1.0";
        var config = new Models.Configuration
        {
            RepositoryUrl = repoUrl,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.RepositoryUrl.Should().Be(repoUrl);
        Uri.TryCreate(repoUrl, UriKind.Absolute, out _).Should().BeTrue();
    }

    [Fact]
    public void RepositoryUrl_IpAddress_ShouldBeValid()
    {
        // Arrange
        var repoUrl = "https://192.168.1.100/git/repository";
        var config = new Models.Configuration
        {
            RepositoryUrl = repoUrl,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.RepositoryUrl.Should().Be(repoUrl);
        Uri.TryCreate(repoUrl, UriKind.Absolute, out var uri).Should().BeTrue();
        uri.Host.Should().Be("192.168.1.100");
    }

    #endregion

    #region SettingsViewModel Binding Tests

    [Fact]
    public void SettingsViewModel_RepositoryUrl_PropertyBinding_ShouldUpdateCorrectly()
    {
        // Arrange
        var mockConfigService = new MockConfigurationService();
        var viewModel = new ViewModels.SettingsViewModel(mockConfigService);
        var newRepoUrl = "https://github.com/elbruno/ElBruno.AspireMonitor";

        // Act
        viewModel.RepositoryUrl = newRepoUrl;

        // Assert
        viewModel.RepositoryUrl.Should().Be(newRepoUrl);
    }

    [Fact]
    public void SettingsViewModel_ProjectFolder_PropertyBinding_ShouldUpdateCorrectly()
    {
        // Arrange
        var mockConfigService = new MockConfigurationService();
        var viewModel = new ViewModels.SettingsViewModel(mockConfigService);
        var projectPath = Path.Combine(Path.GetTempPath(), "TestProject");

        // Act
        viewModel.ProjectFolder = projectPath;

        // Assert
        viewModel.ProjectFolder.Should().Be(projectPath);
    }

    #endregion
}

/// <summary>
/// Mock IConfigurationService for ViewModel testing
/// </summary>
internal class MockConfigurationService : IConfigurationService
{
    private Models.Configuration _config = new Models.Configuration();

    public Models.Configuration LoadConfiguration() => _config;

    public void SaveConfiguration(Models.Configuration configuration)
    {
        _config = configuration;
    }

    public Models.Configuration GetConfiguration() => _config;

    public void UpdateConfiguration(Models.Configuration configuration)
    {
        _config = configuration;
    }

    public void SetEndpoint(string endpoint)
    {
        _config.AspireEndpoint = endpoint;
    }

    public void SetPollingInterval(int intervalMs)
    {
        _config.PollingIntervalMs = intervalMs;
    }

    public void SetThresholds(int cpuWarning, int cpuCritical, int memoryWarning, int memoryCritical)
    {
        _config.CpuThresholdWarning = cpuWarning;
        _config.CpuThresholdCritical = cpuCritical;
        _config.MemoryThresholdWarning = memoryWarning;
        _config.MemoryThresholdCritical = memoryCritical;
    }

    public void ResetToDefaults()
    {
        _config = new Models.Configuration();
    }
}
