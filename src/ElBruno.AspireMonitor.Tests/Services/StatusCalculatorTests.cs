using FluentAssertions;
using Xunit;

namespace ElBruno.AspireMonitor.Tests.Services;

public class StatusCalculatorTests
{
    // Default thresholds: Yellow = 70%, Red = 90%
    private const double DefaultYellowThreshold = 70.0;
    private const double DefaultRedThreshold = 90.0;

    [Theory]
    [InlineData(50, 50, "Green")]
    [InlineData(69, 69, "Green")]
    [InlineData(0, 0, "Green")]
    [InlineData(69.9, 50, "Green")]
    [InlineData(50, 69.9, "Green")]
    public void CalculateStatus_BelowThreshold_ReturnsGreen(double cpu, double memory, string expected)
    {
        // Arrange - both CPU and Memory below 70%
        var statusColor = CalculateStatusColor(cpu, memory, DefaultYellowThreshold, DefaultRedThreshold);
        
        // Assert
        statusColor.Should().Be(expected, 
            $"CPU={cpu}% and Memory={memory}% should be {expected} (below {DefaultYellowThreshold}%)");
    }

    [Theory]
    [InlineData(70, 50, "Yellow")]
    [InlineData(50, 85, "Yellow")]
    [InlineData(89, 89, "Yellow")]
    [InlineData(70.0, 70.0, "Yellow")]
    [InlineData(89.9, 50, "Yellow")]
    [InlineData(50, 89.9, "Yellow")]
    [InlineData(75.5, 80.2, "Yellow")]
    public void CalculateStatus_InWarningRange_ReturnsYellow(double cpu, double memory, string expected)
    {
        // Arrange - either CPU or Memory in 70-89% range
        var statusColor = CalculateStatusColor(cpu, memory, DefaultYellowThreshold, DefaultRedThreshold);
        
        // Assert
        statusColor.Should().Be(expected, 
            $"CPU={cpu}% or Memory={memory}% should be {expected} (between {DefaultYellowThreshold}% and {DefaultRedThreshold}%)");
    }

    [Theory]
    [InlineData(91, 50, "Red")]
    [InlineData(50, 95, "Red")]
    [InlineData(100, 100, "Red")]
    [InlineData(90.0, 50, "Red")]
    [InlineData(50, 90.0, "Red")]
    [InlineData(95.5, 92.3, "Red")]
    public void CalculateStatus_ExceedsCritical_ReturnsRed(double cpu, double memory, string expected)
    {
        // Arrange - either CPU or Memory >= 90%
        var statusColor = CalculateStatusColor(cpu, memory, DefaultYellowThreshold, DefaultRedThreshold);
        
        // Assert
        statusColor.Should().Be(expected, 
            $"CPU={cpu}% or Memory={memory}% should be {expected} (>= {DefaultRedThreshold}%)");
    }

    [Fact]
    public void CalculateStatus_OnError_ReturnsUnknown()
    {
        // Arrange - simulate error state with negative values or NaN
        var statusWithNegative = CalculateStatusColor(-1, 50, DefaultYellowThreshold, DefaultRedThreshold);
        var statusWithNaN = CalculateStatusColor(double.NaN, double.NaN, DefaultYellowThreshold, DefaultRedThreshold);
        
        // Assert
        statusWithNegative.Should().Be("Unknown", "negative values indicate error state");
        statusWithNaN.Should().Be("Unknown", "NaN values indicate error state");
    }

    [Fact]
    public void CalculateStatus_AllThresholdCombinations_WorkCorrectly()
    {
        // Arrange & Act - test boundary conditions
        
        // Exactly at boundaries
        CalculateStatusColor(69.99, 50, 70, 90).Should().Be("Green", "just below yellow threshold");
        CalculateStatusColor(70.0, 50, 70, 90).Should().Be("Yellow", "exactly at yellow threshold");
        CalculateStatusColor(89.99, 50, 70, 90).Should().Be("Yellow", "just below red threshold");
        CalculateStatusColor(90.0, 50, 70, 90).Should().Be("Red", "exactly at red threshold");
        
        // Custom thresholds
        CalculateStatusColor(60, 50, 60, 80).Should().Be("Yellow", "custom thresholds: yellow=60");
        CalculateStatusColor(80, 50, 60, 80).Should().Be("Red", "custom thresholds: red=80");
        CalculateStatusColor(59, 50, 60, 80).Should().Be("Green", "custom thresholds: below yellow");
        
        // Memory-based triggers
        CalculateStatusColor(50, 70, 70, 90).Should().Be("Yellow", "memory triggers yellow");
        CalculateStatusColor(50, 90, 70, 90).Should().Be("Red", "memory triggers red");
        
        // Both high - highest severity wins (Red)
        CalculateStatusColor(95, 95, 70, 90).Should().Be("Red", "both metrics critical");
        CalculateStatusColor(75, 85, 70, 90).Should().Be("Yellow", "both metrics warning");
        CalculateStatusColor(95, 75, 70, 90).Should().Be("Red", "CPU critical overrides memory warning");
    }

    [Theory]
    [InlineData(0, 0, 100, "Green")]  // Both zero
    [InlineData(120, 120, 100, "Red")]  // Both at critical (threshold + 20)
    [InlineData(100, 100, 100, "Yellow")]  // Exactly at custom warning threshold
    [InlineData(50, 50, 50, "Yellow")]  // Exactly at custom threshold
    public void CalculateStatus_EdgeCases_HandledCorrectly(double cpu, double memory, double customThreshold, string expected)
    {
        // Arrange & Act
        var statusColor = CalculateStatusColor(cpu, memory, customThreshold, customThreshold + 20);
        
        // Assert
        statusColor.Should().Be(expected, 
            $"CPU={cpu}%, Memory={memory}% with custom threshold={customThreshold}%");
    }

    [Fact]
    public void CalculateStatus_WithCustomThresholds_RespectsConfiguration()
    {
        // Arrange - test with non-standard thresholds
        double customYellow = 60.0;
        double customRed = 85.0;
        
        // Act & Assert
        CalculateStatusColor(55, 50, customYellow, customRed).Should().Be("Green");
        CalculateStatusColor(65, 50, customYellow, customRed).Should().Be("Yellow");
        CalculateStatusColor(87, 50, customYellow, customRed).Should().Be("Red");
        CalculateStatusColor(50, 62, customYellow, customRed).Should().Be("Yellow");
        CalculateStatusColor(50, 90, customYellow, customRed).Should().Be("Red");
    }

    // Helper method that simulates StatusCalculator logic
    // This will be replaced by actual StatusCalculator when Luke implements it
    private string CalculateStatusColor(double cpuPercent, double memoryPercent, double yellowThreshold, double redThreshold)
    {
        // Handle error states
        if (double.IsNaN(cpuPercent) || double.IsNaN(memoryPercent) || 
            cpuPercent < 0 || memoryPercent < 0)
        {
            return "Unknown";
        }

        // Check for critical (red) state - highest priority
        if (cpuPercent >= redThreshold || memoryPercent >= redThreshold)
        {
            return "Red";
        }

        // Check for warning (yellow) state
        if (cpuPercent >= yellowThreshold || memoryPercent >= yellowThreshold)
        {
            return "Yellow";
        }

        // Normal (green) state
        return "Green";
    }
}
