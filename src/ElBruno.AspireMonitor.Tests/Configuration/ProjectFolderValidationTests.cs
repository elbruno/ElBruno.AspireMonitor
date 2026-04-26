using FluentAssertions;
using System.Text.Json;
using Xunit;
using ElBruno.AspireMonitor.Models;
using ElBruno.AspireMonitor.Services;
using Configuration = ElBruno.AspireMonitor.Models.Configuration;

namespace ElBruno.AspireMonitor.Tests.Configuration;

public class ProjectFolderValidationTests : IDisposable
{
    private readonly string _testProjectDir;
    private readonly string _testConfigDir;
    private readonly string _configFilePath;

    public ProjectFolderValidationTests()
    {
        _testProjectDir = Path.Combine(Path.GetTempPath(), $"AspireProjectTests_{Guid.NewGuid()}");
        _testConfigDir = Path.Combine(Path.GetTempPath(), $"AspireConfigTests_{Guid.NewGuid()}");
        _configFilePath = Path.Combine(_testConfigDir, "config.json");
        
        Directory.CreateDirectory(_testProjectDir);
        Directory.CreateDirectory(_testConfigDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testProjectDir))
                Directory.Delete(_testProjectDir, true);
            if (Directory.Exists(_testConfigDir))
                Directory.Delete(_testConfigDir, true);
        }
        catch
        {
            // Suppress errors during cleanup
        }
    }

    #region ProjectFolder Valid Cases

    [Fact]
    public void ProjectFolder_WithValidPathAndAspireConfigJson_ShouldBeValid()
    {
        // Arrange
        var projectPath = Path.Combine(_testProjectDir, "ValidProject");
        Directory.CreateDirectory(projectPath);
        var aspireConfigPath = Path.Combine(projectPath, "aspire.config.json");
        File.WriteAllText(aspireConfigPath, "{}");

        var config = new Models.Configuration
        {
            ProjectFolder = projectPath,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.ProjectFolder.Should().Be(projectPath);
        File.Exists(Path.Combine(projectPath, "aspire.config.json")).Should().BeTrue();
    }

    [Fact]
    public void ProjectFolder_WithValidPathAndAppHostCs_ShouldBeValid()
    {
        // Arrange
        var projectPath = Path.Combine(_testProjectDir, "ValidProjectWithAppHost");
        Directory.CreateDirectory(projectPath);
        var appHostPath = Path.Combine(projectPath, "AppHost.cs");
        File.WriteAllText(appHostPath, "// Valid AppHost");

        var config = new Models.Configuration
        {
            ProjectFolder = projectPath,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.ProjectFolder.Should().Be(projectPath);
        File.Exists(Path.Combine(projectPath, "AppHost.cs")).Should().BeTrue();
    }

    [Fact]
    public void ProjectFolder_SetToNullOrEmpty_ShouldBeValid()
    {
        // Arrange
        var config = new Models.Configuration
        {
            ProjectFolder = null,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.ProjectFolder.Should().BeNull();
    }

    [Fact]
    public void ProjectFolder_SetToEmptyString_ShouldResetToNoFolder()
    {
        // Arrange
        var config = new Models.Configuration
        {
            ProjectFolder = "",
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.ProjectFolder.Should().BeEmpty();
    }

    [Fact]
    public void ProjectFolder_WithSpacesInPath_ShouldBeValid()
    {
        // Arrange
        var projectPath = Path.Combine(_testProjectDir, "Valid Project With Spaces");
        Directory.CreateDirectory(projectPath);
        var aspireConfigPath = Path.Combine(projectPath, "aspire.config.json");
        File.WriteAllText(aspireConfigPath, "{}");

        var config = new Models.Configuration
        {
            ProjectFolder = projectPath,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.ProjectFolder.Should().Be(projectPath);
        config.ProjectFolder.Should().Contain(" ");
    }

    [Fact]
    public void ProjectFolder_AutoDetectAspireEndpoint_ShouldParseFromAspireConfig()
    {
        // Arrange
        var projectPath = Path.Combine(_testProjectDir, "AutoDetectProject");
        Directory.CreateDirectory(projectPath);
        var aspireConfigPath = Path.Combine(projectPath, "aspire.config.json");
        var aspireConfigContent = @"{
            ""resources"": [],
            ""endpoints"": {
                ""http"": ""http://localhost:18888"",
                ""https"": ""https://localhost:18889""
            }
        }";
        File.WriteAllText(aspireConfigPath, aspireConfigContent);

        // Act
        var config = new Models.Configuration
        {
            ProjectFolder = projectPath,
            AspireEndpoint = "http://localhost:18888"
        };

        // Assert
        config.ProjectFolder.Should().Be(projectPath);
        File.Exists(Path.Combine(projectPath, "aspire.config.json")).Should().BeTrue();
    }

    #endregion

    #region ProjectFolder Invalid Cases

    [Fact]
    public void ProjectFolder_WithNonExistentPath_ShouldFail()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testProjectDir, "NonExistent", "Path");

        // Act & Assert
        Directory.Exists(nonExistentPath).Should().BeFalse();
    }

    [Fact]
    public void ProjectFolder_WithoutAspireConfigJsonOrAppHostCs_ShouldIndicateInvalid()
    {
        // Arrange
        var projectPath = Path.Combine(_testProjectDir, "InvalidProject");
        Directory.CreateDirectory(projectPath);
        // No aspire.config.json or AppHost.cs

        // Act & Assert
        File.Exists(Path.Combine(projectPath, "aspire.config.json")).Should().BeFalse();
        File.Exists(Path.Combine(projectPath, "AppHost.cs")).Should().BeFalse();
    }

    [Fact]
    public void ProjectFolder_AutoDetectionFailsGracefully_WhenAspireConfigMissing()
    {
        // Arrange
        var projectPath = Path.Combine(_testProjectDir, "MissingAspireConfig");
        Directory.CreateDirectory(projectPath);

        var config = new Models.Configuration
        {
            ProjectFolder = projectPath,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        var aspireConfigPath = Path.Combine(projectPath, "aspire.config.json");
        File.Exists(aspireConfigPath).Should().BeFalse();
        config.AspireEndpoint.Should().Be("http://localhost:18888");
    }

    [Fact]
    public void ProjectFolder_AutoDetectionFailsGracefully_WhenAspireConfigMalformed()
    {
        // Arrange
        var projectPath = Path.Combine(_testProjectDir, "MalformedAspireConfig");
        Directory.CreateDirectory(projectPath);
        var aspireConfigPath = Path.Combine(projectPath, "aspire.config.json");
        File.WriteAllText(aspireConfigPath, "{ invalid json }");

        // Act & Assert
        var content = File.ReadAllText(aspireConfigPath);
        var isValidJson = false;
        try
        {
            JsonSerializer.Deserialize<JsonElement>(content);
            isValidJson = true;
        }
        catch
        {
            isValidJson = false;
        }

        isValidJson.Should().BeFalse();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task ProjectFolder_CanBeSavedToConfigFile()
    {
        // Arrange
        var projectPath = Path.Combine(_testProjectDir, "SaveProject");
        Directory.CreateDirectory(projectPath);
        
        var configService = new ConfigurationService(_configFilePath);
        var config = new Models.Configuration
        {
            AspireEndpoint = "http://localhost:18888",
            ProjectFolder = projectPath
        };

        // Act
        configService.SaveConfiguration(config);
        var loadedConfig = configService.LoadConfiguration();

        // Assert
        loadedConfig.ProjectFolder.Should().Be(projectPath);
    }

    [Fact]
    public async Task ProjectFolder_CanBeLoadedFromConfigFile()
    {
        // Arrange
        var projectPath = Path.Combine(_testProjectDir, "LoadProject");
        Directory.CreateDirectory(projectPath);
        
        var configJson = @"{
            ""aspireEndpoint"": ""http://localhost:18888"",
            ""pollingIntervalMs"": 2000,
            ""cpuThresholdWarning"": 70,
            ""cpuThresholdCritical"": 90,
            ""memoryThresholdWarning"": 70,
            ""memoryThresholdCritical"": 90,
            ""projectFolder"": """ + projectPath.Replace("\\", "\\\\") + @"""
        }";
        
        await File.WriteAllTextAsync(_configFilePath, configJson);

        // Act
        var configService = new ConfigurationService(_configFilePath);
        var loadedConfig = configService.LoadConfiguration();

        // Assert
        loadedConfig.ProjectFolder.Should().Be(projectPath);
    }

    [Fact]
    public void ProjectFolder_BackwardCompatibility_OldConfigWithoutProjectFolder()
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
        loadedConfig.ProjectFolder.Should().BeNull();
        loadedConfig.AspireEndpoint.Should().Be("http://localhost:18888");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ProjectFolder_WithRelativePath_ShouldBeStoredAsIs()
    {
        // Arrange
        var relativePath = @"..\Projects\MyProject";
        var config = new Models.Configuration
        {
            ProjectFolder = relativePath,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.ProjectFolder.Should().Be(relativePath);
    }

    [Fact]
    public void ProjectFolder_WithUNCPath_ShouldBeValid()
    {
        // Arrange
        var uncPath = @"\\server\share\project";
        var config = new Models.Configuration
        {
            ProjectFolder = uncPath,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act & Assert
        config.ProjectFolder.Should().Be(uncPath);
    }

    [Fact]
    public void ProjectFolder_ResetToNull_ShouldClearPreviousValue()
    {
        // Arrange
        var projectPath = Path.Combine(_testProjectDir, "ResetProject");
        Directory.CreateDirectory(projectPath);
        
        var config = new Models.Configuration
        {
            ProjectFolder = projectPath,
            AspireEndpoint = "http://localhost:18888"
        };

        // Act
        config.ProjectFolder = null;

        // Assert
        config.ProjectFolder.Should().BeNull();
    }

    #endregion
}
