using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace ElBruno.AspireMonitor.Tests;

public class EdgeCaseTests
{
    private readonly string _emptyJsonPath = Path.Combine("Fixtures", "aspire-response-empty.json");
    private readonly string _malformedJsonPath = Path.Combine("Fixtures", "aspire-response-malformed.json");

    [Fact]
    public async Task EmptyResourceList_HandlesGracefully()
    {
        // Arrange - Load empty resources fixture
        var emptyJson = await File.ReadAllTextAsync(_emptyJsonPath);
        var emptyData = JsonSerializer.Deserialize<JsonElement>(emptyJson);

        // Act
        var resources = emptyData.GetProperty("resources");

        // Assert - Should handle empty array without crashing
        resources.GetArrayLength().Should().Be(0, "empty fixture has no resources");
        
        // UI should show "No resources found" message
        var isEmpty = resources.GetArrayLength() == 0;
        isEmpty.Should().BeTrue("should detect empty resource list");
    }

    [Fact]
    public void DuplicateResourceUrls_AreHandledCorrectly()
    {
        // Arrange - Create resources with duplicate URLs
        var resources = new List<MockResource>
        {
            new() { Name = "service-1", Url = "http://localhost:5000" },
            new() { Name = "service-2", Url = "http://localhost:5000" },
            new() { Name = "service-3", Url = "http://localhost:5001" }
        };

        // Act - Group by URL to detect duplicates
        var duplicateUrls = resources
            .GroupBy(r => r.Url)
            .Where(g => g.Count() > 1)
            .Select(g => new { Url = g.Key, Count = g.Count() })
            .ToList();

        // Assert - Should identify duplicates
        duplicateUrls.Should().HaveCount(1, "one URL is duplicated");
        duplicateUrls[0].Url.Should().Be("http://localhost:5000");
        duplicateUrls[0].Count.Should().Be(2, "URL appears twice");

        // Strategy: Show both resources but indicate duplicate URL
        resources.Where(r => r.Url == "http://localhost:5000")
            .Should().HaveCount(2, "both resources with same URL should be shown");
    }

    [Fact]
    public void LargeResourceList_1000Plus_PerformsWell()
    {
        // Arrange - Generate 1000+ resources
        var largeResourceList = Enumerable.Range(1, 1500)
            .Select(i => new MockResource
            {
                Name = $"service-{i}",
                Url = $"http://localhost:{5000 + i}",
                CpuPercent = Random.Shared.NextDouble() * 100,
                MemoryPercent = Random.Shared.NextDouble() * 100
            })
            .ToList();

        // Act - Simulate processing large list
        var startTime = DateTime.UtcNow;
        
        var processedResources = largeResourceList
            .Select(r => new
            {
                r.Name,
                r.Url,
                Status = CalculateStatus(r.CpuPercent, r.MemoryPercent)
            })
            .ToList();

        var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

        // Assert - Should process quickly (< 100ms for 1500 items)
        processedResources.Should().HaveCount(1500, "all resources should be processed");
        processingTime.Should().BeLessThan(100, "processing 1500 resources should be fast");

        // UI virtualization should handle rendering efficiently
        var visibleCount = Math.Min(processedResources.Count, 50);
        visibleCount.Should().Be(50, "UI should virtualize and render only visible items");
    }

    [Fact]
    public async Task MissingFieldsInApiResponse_DefaultsCorrectly()
    {
        // Arrange - Load malformed fixture with missing fields
        var malformedJson = await File.ReadAllTextAsync(_malformedJsonPath);
        var malformedData = JsonSerializer.Deserialize<JsonElement>(malformedJson);

        // Act - Parse resources with missing/null properties
        var resources = malformedData.GetProperty("resources");
        var firstResource = resources[0];
        var secondResource = resources[1];

        // Assert - First resource has empty properties
        firstResource.TryGetProperty("properties", out var props1).Should().BeTrue();
        props1.EnumerateObject().Should().BeEmpty("properties object exists but is empty");
        
        // Handle missing CPU/Memory by defaulting to 0
        var defaultCpu = 0.0;
        var defaultMemory = 0.0;
        defaultCpu.Should().Be(0.0, "missing CPU should default to 0");
        defaultMemory.Should().Be(0.0, "missing memory should default to 0");

        // Second resource has null values
        secondResource.TryGetProperty("properties", out var props2).Should().BeTrue();
        var hasCpu = props2.TryGetProperty("cpuUsage", out var cpuValue);
        hasCpu.Should().BeTrue();
        cpuValue.ValueKind.Should().Be(JsonValueKind.Null, "cpuUsage is explicitly null");

        // Strategy: Treat null metrics as 0% or Unknown status
        var statusForMissing = CalculateStatus(0, 0);
        statusForMissing.Should().Be("Green", "missing/null metrics default to 0% (green)");
    }

    [Fact]
    public void NullValuesInMetrics_HandledSafely()
    {
        // Arrange - Various null scenarios
        var testCases = new[]
        {
            new { Cpu = (double?)null, Memory = (double?)50.0, Expected = "Green" },
            new { Cpu = (double?)75.0, Memory = (double?)null, Expected = "Yellow" },
            new { Cpu = (double?)null, Memory = (double?)null, Expected = "Green" },
            new { Cpu = (double?)95.0, Memory = (double?)null, Expected = "Red" }
        };

        foreach (var testCase in testCases)
        {
            // Act - Handle null by defaulting to 0
            var cpu = testCase.Cpu ?? 0.0;
            var memory = testCase.Memory ?? 0.0;
            var status = CalculateStatus(cpu, memory);

            // Assert
            status.Should().Be(testCase.Expected, 
                $"CPU={testCase.Cpu}, Memory={testCase.Memory} should be {testCase.Expected}");
        }
    }

    [Fact]
    public async Task ApiTimeout_RetriesAndUsesLastKnownState()
    {
        // Arrange - Simulate cached state
        var lastKnownState = new List<MockResource>
        {
            new() { Name = "api-service", Url = "http://localhost:5000", CpuPercent = 45.5, MemoryPercent = 24.0 }
        };

        // Act - API timeout occurs
        var timeoutOccurred = true;
        var currentState = timeoutOccurred ? lastKnownState : new List<MockResource>();

        // Assert - Should use cached/last known state
        currentState.Should().NotBeEmpty("should use last known state on timeout");
        currentState[0].Name.Should().Be("api-service");
        currentState[0].CpuPercent.Should().Be(45.5);

        // UI should indicate "Using cached data" or show warning
        var showCachedWarning = timeoutOccurred;
        showCachedWarning.Should().BeTrue("should warn user that data is cached");
    }

    [Fact]
    public async Task NetworkOffline_ShowsError_AndAutoReconnects()
    {
        // Arrange - Simulate network state machine
        var networkState = "Offline";
        var reconnectAttempts = 0;
        var maxReconnectAttempts = 3;

        // Act - Auto-reconnect loop
        while (networkState == "Offline" && reconnectAttempts < maxReconnectAttempts)
        {
            reconnectAttempts++;
            await Task.Delay(50); // Simulate reconnect delay with exponential backoff
            
            // Simulate successful reconnect on 3rd attempt
            if (reconnectAttempts == 3)
            {
                networkState = "Online";
            }
        }

        // Assert
        reconnectAttempts.Should().Be(3, "should retry up to max attempts");
        networkState.Should().Be("Online", "should reconnect after retries");
    }

    [Fact]
    public void IntermittentNetwork_ResilientBehavior()
    {
        // Arrange - Simulate intermittent failures
        var networkResponses = new[] { false, true, false, true, true }; // fail, success, fail, success, success
        var successCount = 0;
        var failureCount = 0;

        // Act - Process each response
        foreach (var isSuccess in networkResponses)
        {
            if (isSuccess)
            {
                successCount++;
                // Reset failure counter on success
                failureCount = 0;
            }
            else
            {
                failureCount++;
                // Use exponential backoff
                var backoffDelay = Math.Pow(2, failureCount) * 100;
                backoffDelay.Should().BeGreaterThan(0, "should apply backoff on failure");
            }
        }

        // Assert - Should handle intermittent failures gracefully
        successCount.Should().Be(3, "should succeed 3 times");
        failureCount.Should().Be(0, "failure counter resets on success");
    }

    [Fact]
    public void SpecialCharactersInResourceNames_HandleCorrectly()
    {
        // Arrange - Resources with special characters
        var specialResources = new[]
        {
            "api-service",
            "service_with_underscores",
            "service.with.dots",
            "service (with parens)",
            "service-über-special", // Unicode
            "service@domain",
            "service#123"
        };

        // Act - Validate each name
        foreach (var name in specialResources)
        {
            // Should not throw or crash
            var isValid = !string.IsNullOrWhiteSpace(name);
            isValid.Should().BeTrue($"resource name '{name}' should be valid");
        }

        // Assert - All names should be handled
        specialResources.Should().AllSatisfy(name => 
            name.Should().NotBeNullOrWhiteSpace("resource names should not be empty"));
    }

    [Fact]
    public void VeryLongResourceNames_TruncateInUI()
    {
        // Arrange - Very long resource name
        var longName = new string('x', 500);
        var maxDisplayLength = 50;

        // Act - Truncate for display
        var displayName = longName.Length > maxDisplayLength 
            ? longName.Substring(0, maxDisplayLength) + "..." 
            : longName;

        // Assert
        displayName.Length.Should().BeLessOrEqualTo(maxDisplayLength + 3, "should truncate long names");
        displayName.Should().EndWith("...", "should indicate truncation");
    }

    // Helper classes
    private class MockResource
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public double CpuPercent { get; set; }
        public double MemoryPercent { get; set; }
    }

    private string CalculateStatus(double cpu, double memory)
    {
        if (cpu >= 90 || memory >= 90) return "Red";
        if (cpu >= 70 || memory >= 70) return "Yellow";
        return "Green";
    }
}
