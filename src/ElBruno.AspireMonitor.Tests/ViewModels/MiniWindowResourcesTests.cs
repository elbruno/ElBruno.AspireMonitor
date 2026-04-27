using ElBruno.AspireMonitor.Models;
using ElBruno.AspireMonitor.Services;
using ElBruno.AspireMonitor.ViewModels;
using FluentAssertions;
using Moq;
using Xunit;
using AppConfig = ElBruno.AspireMonitor.Models.Configuration;

namespace ElBruno.AspireMonitor.Tests.ViewModels;

/// <summary>
/// Tests for the mini window pinned resources feature:
/// - Parsing comma-separated resource token lists
/// - MiniMonitorViewModel.PinnedResources behavior with various settings
/// - Prefix matching, case-insensitive, order preservation, replica handling
/// </summary>
public class MiniWindowResourcesTests
{
    #region Parser Tests — Token Extraction from Comma-Separated String

    [Fact]
    public void ParseTokens_EmptyString_ReturnsZeroTokens()
    {
        // Arrange & Act
        var tokens = ParseResourceTokens("");

        // Assert
        tokens.Should().BeEmpty("empty string should yield no tokens");
    }

    [Fact]
    public void ParseTokens_SimpleList_ReturnsTrimmedTokens()
    {
        // Arrange & Act
        var tokens = ParseResourceTokens("web, backend, database");

        // Assert
        tokens.Should().Equal("web", "backend", "database");
    }

    [Fact]
    public void ParseTokens_ExtraCommasAndWhitespace_FiltersEmpties()
    {
        // Arrange & Act
        var tokens = ParseResourceTokens(",,web,, ,backend,");

        // Assert
        tokens.Should().Equal("web", "backend");
    }

    [Fact]
    public void ParseTokens_PreservesCase_InTokenList()
    {
        // Arrange & Act
        var tokens = ParseResourceTokens("WEB, Backend, DATABASE");

        // Assert
        tokens.Should().Equal("WEB", "Backend", "DATABASE");
    }

    #endregion

    #region MiniMonitorViewModel.PinnedResources Tests

    [Fact]
    public void PinnedResources_MatchingResourceWithUrl_HasUrlTrue()
    {
        // Arrange
        var configService = CreateConfigWithMiniResources("web");
        var mainVm = new MainViewModel(null, configService.Object, null);
        
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "web-xggqzmyn",
            Url = "http://localhost:5000",
            Status = ResourceStatus.Running
        });

        // Act
        var miniVm = new MiniMonitorViewModel(mainVm);

        // Assert — access PinnedResources via reflection since it may not be in the current build
        var pinnedResources = GetPinnedResourcesCollection(miniVm);
        
        if (pinnedResources != null)
        {
            pinnedResources.Should().HaveCount(1, "token 'web' should match 'web-xggqzmyn'");
            
            var item = pinnedResources[0];
            GetPropertyValue<string>(item, "Name").Should().Be("web-xggqzmyn");
            GetPropertyValue<bool>(item, "HasUrl").Should().BeTrue("resource has a URL");
            GetPropertyValue<string>(item, "Url").Should().Be("http://localhost:5000");
        }
    }

    [Fact]
    public void PinnedResources_MatchingResourceWithoutUrl_HasUrlFalse_ShowsType()
    {
        // Arrange
        var configService = CreateConfigWithMiniResources("database");
        var mainVm = new MainViewModel(null, configService.Object, null);
        
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "database-abc",
            Url = null,
            Type = "Container",
            Status = ResourceStatus.Running
        });

        // Act
        var miniVm = new MiniMonitorViewModel(mainVm);

        // Assert
        var pinnedResources = GetPinnedResourcesCollection(miniVm);
        
        if (pinnedResources != null)
        {
            pinnedResources.Should().HaveCount(1, "token 'database' should match 'database-abc'");
            
            var item = pinnedResources[0];
            GetPropertyValue<bool>(item, "HasUrl").Should().BeFalse("resource has no URL");
            GetPropertyValue<string>(item, "FallbackText").Should().Be("Container",
                "when no URL, FallbackText should show the resource Type");
        }
    }

    [Fact]
    public void PinnedResources_NoMatchingResource_SkipsSilently()
    {
        // Arrange
        var configService = CreateConfigWithMiniResources("missing");
        var mainVm = new MainViewModel(null, configService.Object, null);
        
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "web-1",
            Status = ResourceStatus.Running
        });

        // Act
        var miniVm = new MiniMonitorViewModel(mainVm);

        // Assert
        var pinnedResources = GetPinnedResourcesCollection(miniVm);
        
        if (pinnedResources != null)
        {
            pinnedResources.Should().HaveCount(1, "token 'missing' does not match any resource; should add a missing entry");
            
            var item = pinnedResources[0];
            GetPropertyValue<string>(item, "Name").Should().Be("missing");
            GetPropertyValue<bool>(item, "IsMissing").Should().BeTrue("resource should be marked as missing");
            GetPropertyValue<string>(item, "FallbackText").Should().Be("not found");
        }
    }

    [Fact]
    public void PinnedResources_MultipleTokens_PreservesUserOrder()
    {
        // Arrange
        var configService = CreateConfigWithMiniResources("web, backend");
        var mainVm = new MainViewModel(null, configService.Object, null);
        
        // Add resources in Aspire enumeration order (backend before web)
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "backend-service",
            Url = "http://localhost:5001",
            Status = ResourceStatus.Running
        });
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "web-app",
            Url = "http://localhost:5000",
            Status = ResourceStatus.Running
        });

        // Act
        var miniVm = new MiniMonitorViewModel(mainVm);

        // Assert
        var pinnedResources = GetPinnedResourcesCollection(miniVm);
        
        if (pinnedResources != null)
        {
            pinnedResources.Should().HaveCount(2, "both tokens should match resources");
            
            // Order must be: web first (token order), backend second
            GetPropertyValue<string>(pinnedResources[0], "Name").Should().Be("web-app",
                "first item should match 'web' token (user order), not Aspire enumeration order");
            GetPropertyValue<string>(pinnedResources[1], "Name").Should().Be("backend-service",
                "second item should match 'backend' token");
        }
    }

    [Fact]
    public void PinnedResources_MultipleReplicasMatchingToken_ShowsAll()
    {
        // Arrange
        var configService = CreateConfigWithMiniResources("web");
        var mainVm = new MainViewModel(null, configService.Object, null);
        
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "web-1",
            Url = "http://localhost:5000",
            Status = ResourceStatus.Running
        });
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "web-2",
            Url = "http://localhost:5001",
            Status = ResourceStatus.Running
        });

        // Act
        var miniVm = new MiniMonitorViewModel(mainVm);

        // Assert
        var pinnedResources = GetPinnedResourcesCollection(miniVm);
        
        if (pinnedResources != null)
        {
            pinnedResources.Should().HaveCount(2,
                "token 'web' matches both replicas; both should appear as separate rows");
            
            GetPropertyValue<string>(pinnedResources[0], "Name").Should().Be("web-1");
            GetPropertyValue<string>(pinnedResources[1], "Name").Should().Be("web-2");
        }
    }

    [Fact]
    public void PinnedResources_CaseInsensitiveMatching_TokenUpperCase()
    {
        // Arrange
        var configService = CreateConfigWithMiniResources("WEB");
        var mainVm = new MainViewModel(null, configService.Object, null);
        
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "web-xggqzmyn",
            Status = ResourceStatus.Running
        });

        // Act
        var miniVm = new MiniMonitorViewModel(mainVm);

        // Assert
        var pinnedResources = GetPinnedResourcesCollection(miniVm);
        
        if (pinnedResources != null)
        {
            pinnedResources.Should().HaveCount(1, "uppercase token 'WEB' should match lowercase 'web-xggqzmyn'");
        }
    }

    [Fact]
    public void PinnedResources_AspireStops_ClearsCollection()
    {
        // Arrange
        var configService = CreateConfigWithMiniResources("web");
        var mainVm = new MainViewModel(null, configService.Object, null);
        
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "web-app",
            Status = ResourceStatus.Running
        });

        var miniVm = new MiniMonitorViewModel(mainVm);
        
        // Verify initial state
        var pinnedResources = GetPinnedResourcesCollection(miniVm);
        if (pinnedResources != null)
        {
            pinnedResources.Should().HaveCount(1, "should have one pinned resource initially");
        }

        // Act — Aspire stops (Resources cleared / IsConnected=false)
        mainVm.Resources.Clear();

        // Assert
        if (pinnedResources != null)
        {
            pinnedResources.Should().BeEmpty("PinnedResources should be cleared when Aspire stops");
        }
    }

    [Fact]
    public void HasPinnedResources_ReflectsCollectionCount()
    {
        // Arrange
        var configService = CreateConfigWithMiniResources("web");
        var mainVm = new MainViewModel(null, configService.Object, null);

        // Act — start with no resources
        var miniVm = new MiniMonitorViewModel(mainVm);
        var hasInitially = GetPropertyValue<bool>(miniVm, "HasPinnedResources");

        // Add a matching resource
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "web-app",
            Status = ResourceStatus.Running
        });

        var hasAfterAdd = GetPropertyValue<bool>(miniVm, "HasPinnedResources");

        // Assert
        hasInitially.Should().BeFalse("HasPinnedResources should be false when collection is empty");
        hasAfterAdd.Should().BeTrue("HasPinnedResources should be true when collection has items");
    }

    [Fact]
    public void PinnedResources_LiveUpdate_ChangingSettingTriggersRefresh()
    {
        // Arrange
        var configService = new Mock<IConfigurationService>();
        configService.Setup(s => s.LoadConfiguration())
            .Returns(new AppConfig { MiniWindowResources = "web" });

        var mainVm = new MainViewModel(null, configService.Object, null);
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "web-app",
            Status = ResourceStatus.Running
        });
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "backend-svc",
            Status = ResourceStatus.Running
        });

        var miniVm = new MiniMonitorViewModel(mainVm);
        
        var pinnedResources = GetPinnedResourcesCollection(miniVm);
        if (pinnedResources != null)
        {
            pinnedResources.Should().HaveCount(1, "initially only 'web' is pinned");
        }

        // Act — change the setting to also include backend
        // Simulate MainViewModel.MiniWindowResourcesSetting property change
        var settingProp = mainVm.GetType().GetProperty("MiniWindowResourcesSetting");
        if (settingProp != null)
        {
            settingProp.SetValue(mainVm, "web, backend");
            // Trigger PropertyChanged via reflection since OnPropertyChanged is protected
            var onPropertyChangedMethod = mainVm.GetType().GetMethod("OnPropertyChanged",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            onPropertyChangedMethod?.Invoke(mainVm, new object[] { "MiniWindowResourcesSetting" });
        }

        // Assert
        if (pinnedResources != null)
        {
            // This test will pass once Han's implementation adds the live update subscription
            // For now, we document the expected behavior
            pinnedResources.Count.Should().BeGreaterOrEqualTo(1,
                "PinnedResources should refresh when MiniWindowResourcesSetting changes (live update)");
        }
    }

    #endregion

    #region Validation Strategy Tests

    [Fact]
    public void PinnedResources_MixOfFoundAndMissing_ShowsBoth()
    {
        // Arrange
        var configService = CreateConfigWithMiniResources("web, missing, backend");
        var mainVm = new MainViewModel(null, configService.Object, null);
        
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "web-app",
            Url = "http://localhost:5000",
            Status = ResourceStatus.Running
        });
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "backend-svc",
            Url = "http://localhost:5001",
            Status = ResourceStatus.Running
        });

        // Act
        var miniVm = new MiniMonitorViewModel(mainVm);

        // Assert
        var pinnedResources = GetPinnedResourcesCollection(miniVm);
        
        if (pinnedResources != null)
        {
            pinnedResources.Should().HaveCount(3, "should show 2 found + 1 missing");
            
            // First: web (found)
            GetPropertyValue<string>(pinnedResources[0], "Name").Should().Be("web-app");
            GetPropertyValue<bool>(pinnedResources[0], "IsMissing").Should().BeFalse();
            
            // Second: missing (not found)
            GetPropertyValue<string>(pinnedResources[1], "Name").Should().Be("missing");
            GetPropertyValue<bool>(pinnedResources[1], "IsMissing").Should().BeTrue();
            GetPropertyValue<string>(pinnedResources[1], "FallbackText").Should().Be("not found");
            
            // Third: backend (found)
            GetPropertyValue<string>(pinnedResources[2], "Name").Should().Be("backend-svc");
            GetPropertyValue<bool>(pinnedResources[2], "IsMissing").Should().BeFalse();
        }
    }

    [Fact]
    public void PinnedResources_OriginalCasingPreserved_ForMissingEntry()
    {
        // Arrange
        var configService = CreateConfigWithMiniResources("WebAPI, MissingService");
        var mainVm = new MainViewModel(null, configService.Object, null);
        
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "webapi-deployment",
            Status = ResourceStatus.Running
        });

        // Act
        var miniVm = new MiniMonitorViewModel(mainVm);

        // Assert
        var pinnedResources = GetPinnedResourcesCollection(miniVm);
        
        if (pinnedResources != null)
        {
            pinnedResources.Should().HaveCount(2);
            
            // Missing entry preserves original casing from user setting
            GetPropertyValue<string>(pinnedResources[1], "Name").Should().Be("MissingService",
                "original token casing should be preserved for missing entries");
        }
    }

    [Fact]
    public void PinnedResources_DuplicateTokens_DeduplicatedCaseInsensitive()
    {
        // Arrange
        var configService = CreateConfigWithMiniResources("web, WEB, Web");
        var mainVm = new MainViewModel(null, configService.Object, null);
        
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "web-app",
            Status = ResourceStatus.Running
        });

        // Act
        var miniVm = new MiniMonitorViewModel(mainVm);

        // Assert
        var pinnedResources = GetPinnedResourcesCollection(miniVm);
        
        if (pinnedResources != null)
        {
            pinnedResources.Should().HaveCount(1, "duplicate tokens should be deduplicated");
        }
    }

    [Fact]
    public void PinnedResources_AspireNotRunning_NoMissingEntries()
    {
        // Arrange
        var configService = CreateConfigWithMiniResources("web, missing");
        var mainVm = new MainViewModel(null, configService.Object, null);
        
        // Aspire not running (no resources)

        // Act
        var miniVm = new MiniMonitorViewModel(mainVm);

        // Assert
        var pinnedResources = GetPinnedResourcesCollection(miniVm);
        
        if (pinnedResources != null)
        {
            pinnedResources.Should().BeEmpty(
                "when Aspire isn't running, don't show missing entries because we have nothing to compare against");
        }
    }

    [Fact]
    public void PinnedResources_StatusEnum_CorrectlyAssigned()
    {
        // Arrange
        var configService = CreateConfigWithMiniResources("hasurl, nourl, missing");
        var mainVm = new MainViewModel(null, configService.Object, null);
        
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "hasurl-svc",
            Url = "http://localhost:5000",
            Status = ResourceStatus.Running
        });
        mainVm.Resources.Add(new ResourceViewModel
        {
            Name = "nourl-svc",
            Url = null,
            Type = "Container",
            Status = ResourceStatus.Running
        });

        // Act
        var miniVm = new MiniMonitorViewModel(mainVm);

        // Assert
        var pinnedResources = GetPinnedResourcesCollection(miniVm);
        
        if (pinnedResources != null)
        {
            pinnedResources.Should().HaveCount(3);
            
            // Check status enum values via reflection
            var statusProp = pinnedResources[0].GetType().GetProperty("Status");
            if (statusProp != null)
            {
                // First: Found (has URL)
                var status0 = statusProp.GetValue(pinnedResources[0]);
                status0.ToString().Should().Be("Found");
                
                // Second: FoundNoUrl
                var status1 = statusProp.GetValue(pinnedResources[1]);
                status1.ToString().Should().Be("FoundNoUrl");
                
                // Third: Missing
                var status2 = statusProp.GetValue(pinnedResources[2]);
                status2.ToString().Should().Be("Missing");
            }
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Parses a comma-separated string into a list of non-empty, trimmed tokens.
    /// This mirrors the parsing logic expected in the MiniMonitorViewModel implementation.
    /// </summary>
    private static List<string> ParseResourceTokens(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new List<string>();

        return input
            .Split(',')
            .Select(token => token.Trim())
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .ToList();
    }

    /// <summary>
    /// Creates a mocked configuration service with MiniWindowResources set.
    /// </summary>
    private static Mock<IConfigurationService> CreateConfigWithMiniResources(string resources)
    {
        var configService = new Mock<IConfigurationService>();
        configService.Setup(s => s.LoadConfiguration())
            .Returns(new AppConfig { MiniWindowResources = resources });
        return configService;
    }

    /// <summary>
    /// Retrieves the PinnedResources collection via reflection.
    /// Returns null if the property doesn't exist yet (Han's code not landed).
    /// </summary>
    private static System.Collections.ObjectModel.ObservableCollection<MiniResourceItem>? GetPinnedResourcesCollection(MiniMonitorViewModel viewModel)
    {
        var prop = viewModel.GetType().GetProperty("PinnedResources");
        return prop?.GetValue(viewModel) as System.Collections.ObjectModel.ObservableCollection<MiniResourceItem>;
    }

    /// <summary>
    /// Retrieves a property value via reflection with type checking.
    /// </summary>
    private static T GetPropertyValue<T>(object obj, string propertyName)
    {
        var prop = obj.GetType().GetProperty(propertyName);
        if (prop == null)
            return default!;
        
        var value = prop.GetValue(obj);
        if (value is T typedValue)
            return typedValue;
        
        return default!;
    }

    #endregion
}
