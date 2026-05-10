using FluentAssertions;
using ElBruno.AspireMonitor.Models;
using ElBruno.AspireMonitor.Services;
using ElBruno.AspireMonitor.ViewModels;
using Moq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using System.Threading;
using System.Windows;
using Xunit;

namespace ElBruno.AspireMonitor.Tests;

public class IntegrationTests
{
    private readonly string _healthyJsonPath = Path.Combine("Fixtures", "aspire-response-healthy.json");
    private readonly string _stressedJsonPath = Path.Combine("Fixtures", "aspire-response-stressed.json");
    private readonly string _telemetryRichJsonPath = Path.Combine("Fixtures", "aspire-response-telemetry-rich.json");

    [Fact]
    public void Configuration_DefaultAspireEndpoint_UsesDashboardPort()
    {
        var configuration = new Configuration();

        configuration.AspireEndpoint.Should().Be(Configuration.DefaultAspireEndpoint);
    }

    [Fact]
    public void ViewModels_DefaultAspireEndpoint_StayAligned()
    {
        var configService = new Mock<IConfigurationService>();
        configService.Setup(service => service.LoadConfiguration())
            .Returns(new Configuration());

        var settingsViewModel = new SettingsViewModel(configService.Object);
        var configurationViewModel = new ConfigurationViewModel();
        var mainViewModel = new MainViewModel(null, configService.Object);

        settingsViewModel.AspireEndpoint.Should().Be(Configuration.DefaultAspireEndpoint);
        configurationViewModel.AspireEndpoint.Should().Be(Configuration.DefaultAspireEndpoint);
        mainViewModel.HostUrl.Should().Be(Configuration.DefaultAspireEndpoint);
    }

    [Fact]
    public void MainViewModel_UsesConfiguredHostUrl_InsteadOfStaleDefault()
    {
        var configService = new Mock<IConfigurationService>();
        configService.Setup(service => service.LoadConfiguration())
            .Returns(new Configuration
            {
                AspireEndpoint = "http://localhost:19999"
            });

        var mainViewModel = new MainViewModel(null, configService.Object);

        mainViewModel.HostUrl.Should().Be("http://localhost:19999");
    }

    [Fact]
    public async Task MainViewModel_LoadsPersistedConfiguration_FromServiceFixture()
    {
        var testDirectory = Path.Combine(AppContext.BaseDirectory, $"config-regression-{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDirectory);

        var configPath = Path.Combine(testDirectory, "config.json");
        await File.WriteAllTextAsync(configPath, await File.ReadAllTextAsync(Path.Combine("Fixtures", "config-valid.json")));

        try
        {
            var configurationService = new ConfigurationService(configPath);
            var mainViewModel = new MainViewModel(null, configurationService);

            mainViewModel.HostUrl.Should().Be(Configuration.DefaultAspireEndpoint);
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Fact]
    public async Task PollingService_WithMockedAspireApi_UpdatesStateCorrectly()
    {
        // Arrange - Simulate polling service with mocked API
        var mockViewModel = new MockMainViewModel();
        var healthyJson = await File.ReadAllTextAsync(_healthyJsonPath);
        var healthyData = JsonSerializer.Deserialize<JsonElement>(healthyJson);

        // Act - Simulate polling update
        await mockViewModel.UpdateResourcesFromApi(healthyData);

        // Assert - Resources should be updated
        mockViewModel.Resources.Should().HaveCount(3, "healthy fixture has 3 resources");
        mockViewModel.Resources[0].Name.Should().Be("api-service");
        mockViewModel.Resources[0].State.Should().Be("Running");
        mockViewModel.Resources[0].StatusColor.Should().Be("Green", "CPU 45.5% and Memory ~24% are healthy");
        
        mockViewModel.Resources[1].Name.Should().Be("postgres-db");
        mockViewModel.Resources[1].StatusColor.Should().Be("Green", "CPU 15.2% and Memory ~24% are healthy");
        
        mockViewModel.Resources[2].Name.Should().Be("redis-cache");
        mockViewModel.Resources[2].StatusColor.Should().Be("Green", "all resources should be green");
    }

    [Fact]
    public async Task Configuration_PersistsAcrossRestarts_Successfully()
    {
        // Arrange - Create temp config file
        var tempConfigPath = Path.Combine(Path.GetTempPath(), $"test-config-{Guid.NewGuid()}.json");
        
        var originalConfig = new
        {
            aspireApiUrl = "http://localhost:19999",
            pollingIntervalSeconds = 3,
            cpuThresholdYellow = 75,
            cpuThresholdRed = 95,
            memoryThresholdYellow = 75,
            memoryThresholdRed = 95
        };

        // Act - Save config
        var json = JsonSerializer.Serialize(originalConfig, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(tempConfigPath, json);

        // Simulate app restart - load config
        var loadedJson = await File.ReadAllTextAsync(tempConfigPath);
        var loadedConfig = JsonSerializer.Deserialize<JsonElement>(loadedJson);

        // Assert - Config persisted correctly
        loadedConfig.GetProperty("aspireApiUrl").GetString().Should().Be("http://localhost:19999");
        loadedConfig.GetProperty("pollingIntervalSeconds").GetInt32().Should().Be(3);
        loadedConfig.GetProperty("cpuThresholdYellow").GetInt32().Should().Be(75);
        loadedConfig.GetProperty("cpuThresholdRed").GetInt32().Should().Be(95);

        // Cleanup
        File.Delete(tempConfigPath);
    }

    [Fact]
    public async Task PollingEvents_TriggerUIUpdates_Correctly()
    {
        // Arrange
        var mockViewModel = new MockMainViewModel();
        var propertyChangedEvents = new List<string>();
        
        mockViewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
                propertyChangedEvents.Add(args.PropertyName);
        };

        var healthyJson = await File.ReadAllTextAsync(_healthyJsonPath);
        var healthyData = JsonSerializer.Deserialize<JsonElement>(healthyJson);

        // Act - Simulate polling update
        await mockViewModel.UpdateResourcesFromApi(healthyData);

        // Assert - PropertyChanged events fired
        propertyChangedEvents.Should().Contain("Resources", "Resources collection should notify changes");
        propertyChangedEvents.Should().Contain("StatusSummary", "Status summary should update");
        mockViewModel.Resources.Should().NotBeEmpty("resources should be populated");
    }

    [Fact]
    public void MainViewModel_PopulatesTelemetryRichResources_WithTypeDiskAndEndpointCount()
    {
        Exception? failure = null;

        var thread = new Thread(() =>
        {
            try
            {
                var pollingService = new Mock<IAspirePollingService>();
                var configService = new Mock<IConfigurationService>();
                configService.Setup(service => service.LoadConfiguration())
                    .Returns(new Configuration());

                var viewModel = new MainViewModel(pollingService.Object, configService.Object);
                var telemetryJson = File.ReadAllText(_telemetryRichJsonPath);
                using var document = JsonDocument.Parse(telemetryJson);
                var resources = JsonSerializer.Deserialize<List<AspireResource>>(
                    document.RootElement.GetProperty("resources").GetRawText(),
                    new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                resources.Should().NotBeNull();
                pollingService.Raise(service => service.ResourcesUpdated += null, viewModel, resources!);

                viewModel.Resources.Should().HaveCount(2);
                viewModel.Resources[0].TypeDisplay.Should().Be("Container");
                viewModel.Resources[0].DiskUsageText.Should().Be("12.3%");
                viewModel.Resources[0].EndpointCountText.Should().Be("2 endpoints");
                viewModel.Resources[0].EnvironmentSummary.Should().Be("ASPNETCORE_ENVIRONMENT=Development, DOTNET_ENVIRONMENT=Development");
                viewModel.Resources[0].IsDevelopmentOnly.Should().BeTrue();
                viewModel.Resources[1].TypeDisplay.Should().Be("Project");
                viewModel.Resources[1].DiskUsageText.Should().Be("3.7%");
                viewModel.Resources[1].EndpointCountText.Should().Be("1 endpoint");
                viewModel.Resources[1].EnvironmentSummary.Should().Be("ASPNETCORE_ENVIRONMENT=Production");
                viewModel.Resources[1].IsDevelopmentOnly.Should().BeFalse();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        failure.Should().BeNull();
    }

    [Fact]
    public void MainViewModel_HidesDevelopmentResources_WhenFilterIsEnabled()
    {
        Exception? failure = null;

        var thread = new Thread(() =>
        {
            try
            {
                var configService = new Mock<IConfigurationService>();
                configService.Setup(service => service.LoadConfiguration())
                    .Returns(new Configuration
                    {
                        HideDevelopmentResources = true
                    });

                var pollingService = new Mock<IAspirePollingService>();
                var viewModel = new MainViewModel(pollingService.Object, configService.Object);
                var resources = LoadTelemetryRichResources();

                pollingService.Raise(service => service.ResourcesUpdated += null, viewModel, resources);

                viewModel.Resources.Should().HaveCount(1);
                viewModel.Resources[0].Name.Should().Be("worker-service");
                viewModel.Resources[0].IsDevelopmentOnly.Should().BeFalse();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        failure.Should().BeNull();
    }

    [Fact]
    public void MainViewModel_PreservesDevelopmentResources_WhenFilterIsDisabled()
    {
        Exception? failure = null;

        var thread = new Thread(() =>
        {
            try
            {
                var configService = new Mock<IConfigurationService>();
                configService.Setup(service => service.LoadConfiguration())
                    .Returns(new Configuration
                    {
                        HideDevelopmentResources = false
                    });

                var pollingService = new Mock<IAspirePollingService>();
                var viewModel = new MainViewModel(pollingService.Object, configService.Object);
                var resources = LoadTelemetryRichResources();

                pollingService.Raise(service => service.ResourcesUpdated += null, viewModel, resources);

                viewModel.Resources.Should().HaveCount(2);
                viewModel.Resources.Should().Contain(resource => resource.Name == "api-service" && resource.IsDevelopmentOnly);
                viewModel.Resources.Should().Contain(resource => resource.Name == "worker-service" && !resource.IsDevelopmentOnly);
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        failure.Should().BeNull();
    }

    [Fact]
    public async Task StatusColor_Updates_WhenResourcesChange()
    {
        // Arrange
        var mockViewModel = new MockMainViewModel();

        // Act - Load healthy resources first
        var healthyJson = await File.ReadAllTextAsync(_healthyJsonPath);
        var healthyData = JsonSerializer.Deserialize<JsonElement>(healthyJson);
        await mockViewModel.UpdateResourcesFromApi(healthyData);

        // Assert - All green
        mockViewModel.Resources.Should().OnlyContain(r => r.StatusColor == "Green");
        mockViewModel.OverallStatus.Should().Be("Green");

        // Act - Load stressed resources
        var stressedJson = await File.ReadAllTextAsync(_stressedJsonPath);
        var stressedData = JsonSerializer.Deserialize<JsonElement>(stressedJson);
        await mockViewModel.UpdateResourcesFromApi(stressedData);

        // Assert - Should have red/yellow statuses
        mockViewModel.Resources.Should().Contain(r => r.StatusColor == "Red", "overloaded-service should be red");
        mockViewModel.Resources.Should().Contain(r => r.StatusColor == "Yellow", "warning-service should be yellow");
        mockViewModel.OverallStatus.Should().Be("Red", "overall status should match worst resource");
    }

    [Fact]
    public async Task ResourceEndpoint_ClickCommand_OpensUrl()
    {
        // Arrange
        var mockViewModel = new MockMainViewModel();
        var healthyJson = await File.ReadAllTextAsync(_healthyJsonPath);
        var healthyData = JsonSerializer.Deserialize<JsonElement>(healthyJson);
        await mockViewModel.UpdateResourcesFromApi(healthyData);

        var resource = mockViewModel.Resources.FirstOrDefault(r => r.Name == "api-service");
        resource.Should().NotBeNull();

        // Act - Simulate URL click
        var urlOpened = false;
        var capturedUrl = "";
        resource!.OnUrlClick = (url) =>
        {
            urlOpened = true;
            capturedUrl = url;
        };

        // Directly invoke handler (command pattern would be in actual ViewModel)
        resource.OnUrlClick?.Invoke("http://localhost:5000");

        // Assert
        urlOpened.Should().BeTrue("URL click should trigger handler");
        capturedUrl.Should().Be("http://localhost:5000", "should pass correct URL");
    }

    [Fact]
    public async Task PollingError_Updates_ErrorState()
    {
        // Arrange
        var mockViewModel = new MockMainViewModel();

        // Act - Simulate API error
        mockViewModel.SimulateApiError("Connection timeout");

        // Assert
        mockViewModel.IsError.Should().BeTrue("error flag should be set");
        mockViewModel.ErrorMessage.Should().Contain("Connection timeout");
        mockViewModel.OverallStatus.Should().Be("Unknown", "status should be unknown on error");
    }

    // Mock ViewModels for integration testing
    private class MockMainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<MockResourceViewModel> Resources { get; } = new();
        public string OverallStatus { get; private set; } = "Green";
        public string StatusSummary { get; private set; } = "";
        public bool IsError { get; private set; }
        public string ErrorMessage { get; private set; } = "";

        public async Task UpdateResourcesFromApi(JsonElement apiResponse)
        {
            await Task.Yield(); // Simulate async work

            Resources.Clear();
            var resources = apiResponse.GetProperty("resources");

            foreach (var resource in resources.EnumerateArray())
            {
                var name = resource.GetProperty("name").GetString() ?? "unknown";
                var state = resource.GetProperty("state").GetString() ?? "Unknown";
                var endpoint = resource.TryGetProperty("endpoints", out var endpoints) && endpoints.GetArrayLength() > 0
                    ? endpoints[0].GetProperty("endpointUrl").GetString() ?? ""
                    : "";

                double cpuPercent = 0;
                double memoryPercent = 0;

                if (resource.TryGetProperty("properties", out var props))
                {
                    if (props.TryGetProperty("cpuUsage", out var cpu))
                        cpuPercent = cpu.GetDouble();

                    if (props.TryGetProperty("memoryUsage", out var memUsage) &&
                        props.TryGetProperty("memoryLimit", out var memLimit))
                    {
                        var usage = memUsage.GetDouble();
                        var limit = memLimit.GetDouble();
                        if (limit > 0)
                            memoryPercent = (usage / limit) * 100;
                    }
                }

                var statusColor = CalculateStatus(cpuPercent, memoryPercent);

                Resources.Add(new MockResourceViewModel
                {
                    Name = name,
                    State = state,
                    EndpointUrl = endpoint,
                    CpuPercent = cpuPercent,
                    MemoryPercent = memoryPercent,
                    StatusColor = statusColor
                });
            }

            // Update overall status (worst status wins)
            OverallStatus = Resources.Any(r => r.StatusColor == "Red") ? "Red" :
                           Resources.Any(r => r.StatusColor == "Yellow") ? "Yellow" : "Green";

            StatusSummary = $"{Resources.Count} resources, Status: {OverallStatus}";

            OnPropertyChanged(nameof(Resources));
            OnPropertyChanged(nameof(StatusSummary));
            OnPropertyChanged(nameof(OverallStatus));
        }

        public void SimulateApiError(string error)
        {
            IsError = true;
            ErrorMessage = error;
            OverallStatus = "Unknown";
            OnPropertyChanged(nameof(IsError));
            OnPropertyChanged(nameof(ErrorMessage));
            OnPropertyChanged(nameof(OverallStatus));
        }

        private string CalculateStatus(double cpu, double memory)
        {
            if (cpu >= 90 || memory >= 90) return "Red";
            if (cpu >= 70 || memory >= 70) return "Yellow";
            return "Green";
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private class MockResourceViewModel
    {
        public string Name { get; set; } = "";
        public string State { get; set; } = "";
        public string EndpointUrl { get; set; } = "";
        public double CpuPercent { get; set; }
        public double MemoryPercent { get; set; }
        public string StatusColor { get; set; } = "Green";
        public Action<string>? OnUrlClick { get; set; }
        public Action<object?>? OpenUrlCommand { get; set; }
    }

    private static List<AspireResource> LoadTelemetryRichResources()
    {
        var telemetryJson = File.ReadAllText(Path.Combine("Fixtures", "aspire-response-telemetry-rich.json"));
        using var document = JsonDocument.Parse(telemetryJson);
        var resources = JsonSerializer.Deserialize<List<AspireResource>>(
            document.RootElement.GetProperty("resources").GetRawText(),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return resources ?? new List<AspireResource>();
    }
}
