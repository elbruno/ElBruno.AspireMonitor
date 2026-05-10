using FluentAssertions;
using ElBruno.AspireMonitor.ViewModels;
using Xunit;

namespace ElBruno.AspireMonitor.Tests.ViewModels;

public class ResourceViewModelTests
{
    [Fact]
    public void TypeDisplay_UsesResourceType_WhenPresent()
    {
        var viewModel = new ResourceViewModel
        {
            Type = "Container"
        };

        viewModel.TypeDisplay.Should().Be("Container");
    }

    [Fact]
    public void DiskUsageText_FormatsAsPercentage()
    {
        var viewModel = new ResourceViewModel
        {
            DiskUsage = 12.34
        };

        viewModel.DiskUsageText.Should().Be("12.3%");
    }

    [Fact]
    public void EndpointCountText_UsesSingularAndPluralForms()
    {
        var singleEndpoint = new ResourceViewModel
        {
            EndpointCount = 1
        };

        var multipleEndpoints = new ResourceViewModel
        {
            EndpointCount = 2
        };

        singleEndpoint.EndpointCountText.Should().Be("1 endpoint");
        multipleEndpoints.EndpointCountText.Should().Be("2 endpoints");
    }
}
