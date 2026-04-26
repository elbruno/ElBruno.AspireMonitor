using FluentAssertions;
using System.Text.Json;
using Xunit;
using ElBruno.AspireMonitor.Models;
using ElBruno.AspireMonitor.Services;
using Configuration = ElBruno.AspireMonitor.Models.Configuration;

namespace ElBruno.AspireMonitor.Tests.Configuration;

public class ProjectFolderRepositoryUrlIntegrationTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _configFilePath;

    public ProjectFolderRepositoryUrlIntegrationTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"AspireIntegrationTests_{Guid.NewGuid()}");
        _configFilePath = Path.Combine(_testDir, "config.json");
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, true);
        }
        catch
        {
            // Suppress errors during cleanup
        }
    }

    #region Combined Save and Load Tests

    [Fact]
    public void SaveAndLoad_BothFields_ShouldPersistCorrectly()
    {
        // Arrange
        var projectPath = Path.Combine(_testDir, "MyProject");
        Directory.CreateDirectory(projectPath);
        var repoUrl = "https://github.com/elbruno/ElBruno.AspireMonitor";

        var configService = new ConfigurationService(_configFilePath);
        var originalConfig = new Models.Configuration
        {
            AspireEndpoint = "http://localhost:18888",
            PollingIntervalMs = 5000,
            CpuThresholdWarning = 70,
            CpuThresholdCritical = 90,
            MemoryThresholdWarning = 70,
            MemoryThresholdCritical = 90,
            ProjectFolder = projectPath,
            RepositoryUrl = repoUrl
        };

        // Act
        configService.SaveConfiguration(originalConfig);
        var loadedConfig = configService.LoadConfiguration();

        // Assert
        loadedConfig.ProjectFolder.Should().Be(projectPath);
        loadedConfig.RepositoryUrl.Should().Be(repoUrl);
        loadedConfig.AspireEndpoint.Should().Be("http://localhost:18888");
        loadedConfig.PollingIntervalMs.Should().Be(5000);
    }

    [Fact]
    public void SaveAndLoad_OnlyProjectFolder_ShouldPersistWithNullRepositoryUrl()
    {
        // Arrange
        var projectPath = Path.Combine(_testDir, "MyProject");
        Directory.CreateDirectory(projectPath);

        var configService = new ConfigurationService(_configFilePath);
        var originalConfig = new Models.Configuration
        {
            AspireEndpoint = "http://localhost:18888",
            ProjectFolder = projectPath,
            RepositoryUrl = null
        };

        // Act
        configService.SaveConfiguration(originalConfig);
        var loadedConfig = configService.LoadConfiguration();

        // Assert
        loadedConfig.ProjectFolder.Should().Be(projectPath);
        loadedConfig.RepositoryUrl.Should().BeNull();
    }

    [Fact]
    public void SaveAndLoad_OnlyRepositoryUrl_ShouldPersistWithNullProjectFolder()
    {
        // Arrange
        var repoUrl = "https://github.com/elbruno/ElBruno.AspireMonitor";

        var configService = new ConfigurationService(_configFilePath);
        var originalConfig = new Models.Configuration
        {
            AspireEndpoint = "http://localhost:18888",
            ProjectFolder = null,
            RepositoryUrl = repoUrl
        };

        // Act
        configService.SaveConfiguration(originalConfig);
        var loadedConfig = configService.LoadConfiguration();

        // Assert
        loadedConfig.ProjectFolder.Should().BeNull();
        loadedConfig.RepositoryUrl.Should().Be(repoUrl);
    }

    #endregion

    #region Backward Compatibility Tests

    [Fact]
    public void Load_OldConfigFile_WithoutBothNewFields_ShouldUseDefaults()
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
        loadedConfig.AspireEndpoint.Should().Be("http://localhost:18888");
        loadedConfig.ProjectFolder.Should().BeNull();
        loadedConfig.RepositoryUrl.Should().BeNull();
    }

    [Fact]
    public void Load_PartialOldConfig_WithOnlyProjectFolder_ShouldLoadPartially()
    {
        // Arrange
        var projectPath = Path.Combine(_testDir, "PartialProject");
        Directory.CreateDirectory(projectPath);
        
        var oldConfigJson = @"{
            ""aspireEndpoint"": ""http://localhost:18888"",
            ""pollingIntervalMs"": 2000,
            ""cpuThresholdWarning"": 70,
            ""cpuThresholdCritical"": 90,
            ""memoryThresholdWarning"": 70,
            ""memoryThresholdCritical"": 90,
            ""projectFolder"": """ + projectPath.Replace("\\", "\\\\") + @"""
        }";
        
        File.WriteAllText(_configFilePath, oldConfigJson);

        // Act
        var configService = new ConfigurationService(_configFilePath);
        var loadedConfig = configService.LoadConfiguration();

        // Assert
        loadedConfig.ProjectFolder.Should().Be(projectPath);
        loadedConfig.RepositoryUrl.Should().BeNull();
    }

    [Fact]
    public void Load_ConfigWithExtraProperties_ShouldIgnoreAndLoadKnownFields()
    {
        // Arrange
        var projectPath = Path.Combine(_testDir, "TestProject");
        Directory.CreateDirectory(projectPath);
        
        var configJsonWithExtra = @"{
            ""aspireEndpoint"": ""http://localhost:18888"",
            ""pollingIntervalMs"": 2000,
            ""cpuThresholdWarning"": 70,
            ""cpuThresholdCritical"": 90,
            ""memoryThresholdWarning"": 70,
            ""memoryThresholdCritical"": 90,
            ""projectFolder"": """ + projectPath.Replace("\\", "\\\\") + @""",
            ""futureProperty"": ""some value""
        }";
        
        File.WriteAllText(_configFilePath, configJsonWithExtra);

        // Act
        var configService = new ConfigurationService(_configFilePath);
        var loadedConfig = configService.LoadConfiguration();

        // Assert
        loadedConfig.ProjectFolder.Should().Be(projectPath);
        loadedConfig.AspireEndpoint.Should().Be("http://localhost:18888");
    }

    #endregion

    #region SettingsViewModel Integration Tests

    [Fact]
    public void SettingsViewModel_LoadsProjectFolder_FromConfigService()
    {
        // Arrange
        var projectPath = Path.Combine(_testDir, "ViewModelProject");
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
        
        File.WriteAllText(_configFilePath, configJson);
        var configService = new ConfigurationService(_configFilePath);

        // Act
        var viewModel = new ViewModels.SettingsViewModel(configService);

        // Assert
        viewModel.ProjectFolder.Should().Be(projectPath);
    }

    [Fact]
    public void SettingsViewModel_LoadsRepositoryUrl_FromConfigService()
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
        
        File.WriteAllText(_configFilePath, configJson);
        var configService = new ConfigurationService(_configFilePath);

        // Act
        var viewModel = new ViewModels.SettingsViewModel(configService);

        // Assert
        viewModel.RepositoryUrl.Should().Be(repoUrl);
    }

    [Fact]
    public void SettingsViewModel_SavesProjectFolder_ToConfigService()
    {
        // Arrange
        var projectPath = Path.Combine(_testDir, "SaveProject");
        Directory.CreateDirectory(projectPath);
        
        var configService = new ConfigurationService(_configFilePath);
        var viewModel = new ViewModels.SettingsViewModel(configService);

        // Act
        viewModel.ProjectFolder = projectPath;
        viewModel.SaveSettings();
        
        var loadedConfig = configService.LoadConfiguration();

        // Assert
        loadedConfig.ProjectFolder.Should().Be(projectPath);
    }

    [Fact]
    public void SettingsViewModel_SavesRepositoryUrl_ToConfigService()
    {
        // Arrange
        var repoUrl = "https://github.com/elbruno/ElBruno.AspireMonitor";
        var configService = new ConfigurationService(_configFilePath);
        var viewModel = new ViewModels.SettingsViewModel(configService);

        // Act
        viewModel.RepositoryUrl = repoUrl;
        viewModel.SaveSettings();
        
        var loadedConfig = configService.LoadConfiguration();

        // Assert
        loadedConfig.RepositoryUrl.Should().Be(repoUrl);
    }

    [Fact]
    public void SettingsViewModel_SavesBothFields_ToConfigService()
    {
        // Arrange
        var projectPath = Path.Combine(_testDir, "BothFieldsProject");
        Directory.CreateDirectory(projectPath);
        var repoUrl = "https://github.com/elbruno/ElBruno.AspireMonitor";
        
        var configService = new ConfigurationService(_configFilePath);
        var viewModel = new ViewModels.SettingsViewModel(configService);

        // Act
        viewModel.ProjectFolder = projectPath;
        viewModel.RepositoryUrl = repoUrl;
        viewModel.SaveSettings();
        
        var loadedConfig = configService.LoadConfiguration();

        // Assert
        loadedConfig.ProjectFolder.Should().Be(projectPath);
        loadedConfig.RepositoryUrl.Should().Be(repoUrl);
    }

    #endregion

    #region Edge Cases and Error Scenarios

    [Fact]
    public void LoadAndModify_ExistingConfig_WithBothFields_ShouldPreserveOnResave()
    {
        // Arrange
        var originalProjectPath = Path.Combine(_testDir, "OriginalProject");
        Directory.CreateDirectory(originalProjectPath);
        var originalRepoUrl = "https://github.com/original/repo";

        var configService = new ConfigurationService(_configFilePath);
        var originalConfig = new Models.Configuration
        {
            AspireEndpoint = "http://localhost:18888",
            ProjectFolder = originalProjectPath,
            RepositoryUrl = originalRepoUrl
        };

        configService.SaveConfiguration(originalConfig);

        // Act - Modify only the repository URL
        var loadedConfig = configService.LoadConfiguration();
        var newRepoUrl = "https://github.com/new/repo";
        loadedConfig.RepositoryUrl = newRepoUrl;
        configService.SaveConfiguration(loadedConfig);

        var reloadedConfig = configService.LoadConfiguration();

        // Assert
        reloadedConfig.ProjectFolder.Should().Be(originalProjectPath); // Preserved
        reloadedConfig.RepositoryUrl.Should().Be(newRepoUrl); // Updated
    }

    [Fact]
    public void EmptyStringsForBothFields_ShouldBeDistinguishedFromNull()
    {
        // Arrange
        var configService = new ConfigurationService(_configFilePath);
        var config = new Models.Configuration
        {
            AspireEndpoint = "http://localhost:18888",
            ProjectFolder = "",
            RepositoryUrl = ""
        };

        // Act
        configService.SaveConfiguration(config);
        var loadedConfig = configService.LoadConfiguration();

        // Assert
        loadedConfig.ProjectFolder.Should().Be("");
        loadedConfig.RepositoryUrl.Should().Be("");
    }

    [Fact]
    public void WhitespaceOnlyValues_ShouldBeTreatedAsProvidedValues()
    {
        // Arrange
        var configService = new ConfigurationService(_configFilePath);
        var config = new Models.Configuration
        {
            AspireEndpoint = "http://localhost:18888",
            ProjectFolder = "   ",
            RepositoryUrl = "   "
        };

        // Act
        configService.SaveConfiguration(config);
        var loadedConfig = configService.LoadConfiguration();

        // Assert
        loadedConfig.ProjectFolder.Should().Be("   ");
        loadedConfig.RepositoryUrl.Should().Be("   ");
    }

    [Fact]
    public void ConfigFileDamaged_ShouldFallbackToDefaults()
    {
        // Arrange
        File.WriteAllText(_configFilePath, "{ incomplete json");

        // Act
        var configService = new ConfigurationService(_configFilePath);
        var loadedConfig = configService.LoadConfiguration();

        // Assert
        loadedConfig.AspireEndpoint.Should().Be("http://localhost:15888"); // Default
        loadedConfig.ProjectFolder.Should().BeNull();
        loadedConfig.RepositoryUrl.Should().BeNull();
    }

    [Fact]
    public void MultipleReadsAfterSave_ShouldReturnConsistentData()
    {
        // Arrange
        var projectPath = Path.Combine(_testDir, "ConsistencyProject");
        Directory.CreateDirectory(projectPath);
        var repoUrl = "https://github.com/elbruno/ElBruno.AspireMonitor";

        var configService = new ConfigurationService(_configFilePath);
        var config = new Models.Configuration
        {
            AspireEndpoint = "http://localhost:18888",
            ProjectFolder = projectPath,
            RepositoryUrl = repoUrl
        };

        configService.SaveConfiguration(config);

        // Act
        var firstRead = configService.LoadConfiguration();
        var secondRead = configService.LoadConfiguration();
        var thirdRead = configService.LoadConfiguration();

        // Assert
        firstRead.ProjectFolder.Should().Be(projectPath);
        firstRead.RepositoryUrl.Should().Be(repoUrl);
        
        secondRead.ProjectFolder.Should().Be(firstRead.ProjectFolder);
        secondRead.RepositoryUrl.Should().Be(firstRead.RepositoryUrl);
        
        thirdRead.ProjectFolder.Should().Be(secondRead.ProjectFolder);
        thirdRead.RepositoryUrl.Should().Be(secondRead.RepositoryUrl);
    }

    #endregion
}
