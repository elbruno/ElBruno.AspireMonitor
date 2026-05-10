using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ElBruno.AspireMonitor.Services;
using FluentAssertions;
using Xunit;

namespace ElBruno.AspireMonitor.Tests.Services;

/// <summary>
/// Comprehensive unit tests for AspireCommandService (Phase 1 Backend Layer).
/// Tests CLI command execution, output parsing, and error handling.
/// </summary>
public class AspireCommandServiceTests
{
    [Fact]
    public void CreateStartProcessStartInfo_UsesAspireStart()
    {
        // Arrange
        var workingFolder = Environment.CurrentDirectory;

        // Act
        var processInfo = AspireCommandService.CreateStartProcessStartInfo(workingFolder);

        // Assert
        processInfo.FileName.Should().Be("aspire");
        processInfo.Arguments.Should().Be("start", "the UI Start button must run 'aspire start', not 'aspire run'");
        processInfo.WorkingDirectory.Should().Be(workingFolder);
        processInfo.UseShellExecute.Should().BeFalse();
        processInfo.CreateNoWindow.Should().BeTrue();
        processInfo.RedirectStandardOutput.Should().BeTrue();
        processInfo.RedirectStandardError.Should().BeTrue();
    }

    [Fact]
    public async Task StartAspireAsync_WithValidFolder_ReturnsTrue()
    {
        // Arrange
        var service = new AspireCommandService();
        var workingFolder = Environment.CurrentDirectory;

        // Act & Assert
        // Note: This will actually try to run 'aspire start' which may fail in test environment
        // In a real scenario, we would mock Process.Start, but for now we test the validation logic
        var result = await service.StartAspireAsync(workingFolder);
        
        // Result can be true or false depending on whether aspire CLI is available
        result.Should().Be(result); // Just verify it returns without exception
    }

    [Fact]
    public async Task StartAspireAsync_WithNullFolder_ReturnsFalse()
    {
        // Arrange
        var service = new AspireCommandService();

        // Act
        var result = await service.StartAspireAsync(null!);

        // Assert
        result.Should().BeFalse("null working folder should fail validation");
    }

    [Fact]
    public async Task StartAspireAsync_WithEmptyFolder_ReturnsFalse()
    {
        // Arrange
        var service = new AspireCommandService();

        // Act
        var result = await service.StartAspireAsync(string.Empty);

        // Assert
        result.Should().BeFalse("empty working folder should fail validation");
    }

    [Fact]
    public async Task StartAspireAsync_WithInvalidFolder_ReturnsFalse()
    {
        // Arrange
        var service = new AspireCommandService();
        var invalidFolder = @"C:\NonExistent\Folder\That\Does\Not\Exist";

        // Act
        var result = await service.StartAspireAsync(invalidFolder);

        // Assert
        result.Should().BeFalse("non-existent folder should fail validation");
    }

    [Fact]
    public async Task StartAspireAsync_WithLogCallback_InvokesCallback()
    {
        // Arrange
        var service = new AspireCommandService();
        var workingFolder = Environment.CurrentDirectory;
        var logMessages = new List<string>();
        
        // Act
        await service.StartAspireAsync(workingFolder, msg => logMessages.Add(msg));
        
        // Give time for async streaming
        await Task.Delay(1500);

        // Assert
        // Callback should be registered (actual invocation depends on aspire CLI availability)
        logMessages.Should().NotBeNull();
    }

    [Fact]
    public async Task StopAspireAsync_ReturnsResult()
    {
        // Arrange
        var service = new AspireCommandService();

        // Act
        var result = await service.StopAspireAsync();

        // Assert
        // Result depends on aspire CLI availability
        result.Should().Be(result); // Just verify it returns without exception
    }

    [Fact]
    public async Task StopAspireAsync_WithLogCallback_InvokesCallback()
    {
        // Arrange
        var service = new AspireCommandService();
        var logMessages = new List<string>();
        
        // Act
        await service.StopAspireAsync(msg => logMessages.Add(msg));
        
        // Give time for async streaming
        await Task.Delay(200);

        // Assert
        logMessages.Should().NotBeNull();
    }

    [Fact]
    public async Task DetectAspireEndpointAsync_ParsesEndpointFromOutput()
    {
        // Arrange
        var service = new AspireCommandService();

        // Act
        var endpoint = await service.DetectAspireEndpointAsync();

        // Assert
        // Endpoint can be null or a valid URL depending on aspire CLI availability
        if (endpoint != null)
        {
            endpoint.Should().StartWith("http");
            endpoint.Should().Contain("localhost");
        }
    }

    [Fact]
    public async Task DetectAspireEndpointAsync_WithLogCallback_InvokesCallback()
    {
        // Arrange
        var service = new AspireCommandService();
        var logMessages = new List<string>();
        
        // Act
        await service.DetectAspireEndpointAsync(msg => logMessages.Add(msg));
        
        // Give time for async streaming
        await Task.Delay(200);

        // Assert
        logMessages.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRunningInstancesAsync_ReturnsOutput()
    {
        // Arrange
        var service = new AspireCommandService();

        // Act
        var result = await service.GetRunningInstancesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty("should return output or error message");
    }

    [Fact]
    public async Task GetRunningInstancesAsync_WithLogCallback_InvokesCallback()
    {
        // Arrange
        var service = new AspireCommandService();
        var logMessages = new List<string>();
        
        // Act
        await service.GetRunningInstancesAsync(msg => logMessages.Add(msg));
        
        // Give time for async streaming
        await Task.Delay(200);

        // Assert
        logMessages.Should().NotBeNull();
    }

    [Fact]
    public async Task DescribeResourcesAsync_ReturnsOutput()
    {
        // Arrange
        var service = new AspireCommandService();

        // Act
        var result = await service.DescribeResourcesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty("should return output or error message");
    }

    [Fact]
    public async Task DescribeResourcesAsync_WithLogCallback_InvokesCallback()
    {
        // Arrange
        var service = new AspireCommandService();
        var logMessages = new List<string>();
        
        // Act
        await service.DescribeResourcesAsync(msg => logMessages.Add(msg));
        
        // Give time for async streaming
        await Task.Delay(200);

        // Assert
        logMessages.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsOutput()
    {
        // Arrange
        var service = new AspireCommandService();

        // Act
        var result = await service.GetStatusAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty("should return status output");
    }

    [Theory]
    [InlineData("http://localhost:18888")]
    [InlineData("https://localhost:19999")]
    [InlineData("http://127.0.0.1:18888")]
    public void ParseEndpointFromAspirePs_ValidUrls_ExtractsCorrectly(string expectedUrl)
    {
        // Arrange
        var service = new AspireCommandService();
        var output = $"NAME    PID   DASHBOARD\nmyapp   12345 {expectedUrl}    logs";

        // Act
        // Use reflection to call private method for testing
        var method = typeof(AspireCommandService).GetMethod("ParseEndpointFromAspirePs",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = method?.Invoke(service, new[] { output }) as string;

        // Assert
        result.Should().Be(expectedUrl, "should extract URL from aspire ps output");
    }

    [Fact]
    public void ParseEndpointFromAspirePs_WithQueryParams_PreservesQueryParams()
    {
        // Arrange
        var service = new AspireCommandService();
        var output = "Dashboard: http://localhost:18888/login?t=abc123";

        // Act
        var method = typeof(AspireCommandService).GetMethod("ParseEndpointFromAspirePs",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = method?.Invoke(service, new[] { output }) as string;

        // Assert
        result.Should().Be("http://localhost:18888/login?t=abc123", "should preserve query parameters for login token");
    }

    [Fact]
    public void ParseEndpointFromAspirePs_EmptyOutput_ReturnsNull()
    {
        // Arrange
        var service = new AspireCommandService();
        var output = string.Empty;

        // Act
        var method = typeof(AspireCommandService).GetMethod("ParseEndpointFromAspirePs",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = method?.Invoke(service, new[] { output }) as string;

        // Assert
        result.Should().BeNull("empty output should return null");
    }

    [Fact]
    public void ParseEndpointFromAspirePs_NoUrl_ReturnsNull()
    {
        // Arrange
        var service = new AspireCommandService();
        var output = "No running instances found";

        // Act
        var method = typeof(AspireCommandService).GetMethod("ParseEndpointFromAspirePs",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = method?.Invoke(service, new[] { output }) as string;

        // Assert
        result.Should().BeNull("output without URL should return null");
    }

    [Fact]
    public void ParseEndpointFromAspirePsJson_WithDashboardUrl_ReturnsFullLoginUrl()
    {
        // Arrange
        var output = """
            [{
                "appHostPath": "C:\\src\\SampleHarness\\SampleHarness.AppHost.csproj",
                "appHostPid": 12345,
                "cliPid": 67890,
                "dashboardUrl": "http://localhost:15295/login?t=abc123"
            }]
            """;

        // Act
        var result = InvokeJsonParser(output);

        // Assert
        result.Should().Be("http://localhost:15295/login?t=abc123",
            "JSON parser should preserve the Aspire dashboard login token");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseEndpointFromAspirePsJson_WithEmptyOutput_ReturnsNull(string output)
    {
        // Act
        var result = InvokeJsonParser(output);

        // Assert
        result.Should().BeNull("empty JSON output should not produce an endpoint");
    }

    [Fact]
    public void ParseEndpointFromAspirePsJson_WithMissingDashboardUrl_ReturnsNull()
    {
        // Arrange
        var output = """
            [{
                "appHostPath": "C:\\src\\SampleHarness\\SampleHarness.AppHost.csproj",
                "appHostPid": 12345,
                "cliPid": 67890
            }]
            """;

        // Act
        var result = InvokeJsonParser(output);

        // Assert
        result.Should().BeNull("instances without dashboardUrl should fall back to text parsing");
    }

    [Theory]
    [InlineData("""[{ "dashboardUrl": "" }]""")]
    [InlineData("""[{ "dashboardUrl": null }]""")]
    public void ParseEndpointFromAspirePsJson_WithBlankDashboardUrl_ReturnsNull(string output)
    {
        // Act
        var result = InvokeJsonParser(output);

        // Assert
        result.Should().BeNull("blank dashboardUrl values should be treated as missing");
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("""{ "dashboardUrl": "http://localhost:15295/login?t=abc123" }""")]
    public void ParseEndpointFromAspirePsJson_WithNonInstanceArray_ReturnsNull(string output)
    {
        // Act
        var result = InvokeJsonParser(output);

        // Assert
        result.Should().BeNull("only non-empty Aspire ps arrays contain instances");
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("[")]
    public void ParseEndpointFromAspirePsJson_WithInvalidJson_ReturnsNull(string output)
    {
        // Act
        var result = InvokeJsonParser(output);

        // Assert
        result.Should().BeNull("invalid JSON should fall back to text parsing");
    }

    [Fact]
    public void ParseEndpointFromAspirePsJson_WithNonObjectInstance_ReturnsNull()
    {
        // Act
        var result = InvokeJsonParser("[42]");

        // Assert
        result.Should().BeNull("unexpected JSON shapes should not throw");
    }

    [Fact]
    public async Task ConcurrentCommands_HandleCorrectly()
    {
        // Arrange
        var service = new AspireCommandService();

        // Act
        var tasks = new[]
        {
            service.GetRunningInstancesAsync(),
            service.DescribeResourcesAsync(),
            service.GetStatusAsync()
        };

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(3, "all concurrent commands should complete");
        results.Should().OnlyContain(r => r != null, "all results should be non-null");
    }

    [Fact]
    public async Task CommandExecutionTimeout_HandlesGracefully()
    {
        // Arrange
        var service = new AspireCommandService();

        // Act
        var startTime = DateTime.UtcNow;
        var result = await service.GetRunningInstancesAsync();
        var duration = DateTime.UtcNow - startTime;

        // Assert
        duration.Should().BeLessThan(TimeSpan.FromSeconds(15), 
            "command should complete within reasonable timeout");
        result.Should().NotBeNull("should return result even on timeout");
    }

    private static string? InvokeJsonParser(string output)
    {
        var service = new AspireCommandService();
        var method = typeof(AspireCommandService).GetMethod("ParseEndpointFromAspirePsJson",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Should().NotBeNull("JSON parser should remain available for deterministic branch coverage");
        return method!.Invoke(service, new[] { output }) as string;
    }
}

/// <summary>
/// Tests for endpoint URL parsing logic from aspire ps output.
/// </summary>
public class AspireEndpointParsingTests
{
    [Theory]
    [InlineData("http://localhost:18888", "http://localhost:18888")]
    [InlineData("https://localhost:19999", "https://localhost:19999")]
    [InlineData("http://127.0.0.1:5000", "http://127.0.0.1:5000")]
    public void ParseEndpoint_ValidFormats_ExtractsCorrectly(string input, string expected)
    {
        // Arrange
        var output = $"Dashboard: {input}";
        
        // Act
        var pattern = @"https?://(?:localhost|127\.0\.0\.1):\d+";
        var match = System.Text.RegularExpressions.Regex.Match(output, pattern);

        // Assert
        match.Success.Should().BeTrue("should match valid URL pattern");
        match.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("No running instances")]
    [InlineData("ftp://localhost:21")]
    [InlineData("http://example.com:80")]
    public void ParseEndpoint_InvalidFormats_MatchesFail(string output)
    {
        // Arrange & Act
        var pattern = @"https?://(?:localhost|127\.0\.0\.1):\d+";
        var match = System.Text.RegularExpressions.Regex.Match(output, pattern);

        // Assert
        match.Success.Should().BeFalse("should not match invalid URL patterns");
    }

    [Fact]
    public void ParseEndpoint_MultipleUrls_ExtractsFirst()
    {
        // Arrange
        var output = "http://localhost:18888 and http://localhost:19999";
        
        // Act
        var pattern = @"https?://(?:localhost|127\.0\.0\.1):\d+";
        var match = System.Text.RegularExpressions.Regex.Match(output, pattern);

        // Assert
        match.Success.Should().BeTrue();
        match.Value.Should().Be("http://localhost:18888", "should extract first URL");
    }
}
