using FluentAssertions;
using ElBruno.AspireMonitor.Models;
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

    [Fact]
    public void EnvironmentSummary_FormatsEnvironmentPairs()
    {
        var viewModel = new ResourceViewModel
        {
            Environment = new List<AspireEnvironmentEntry>
            {
                new()
                {
                    Name = "ASPNETCORE_ENVIRONMENT",
                    Value = "Development"
                },
                new()
                {
                    Name = "DOTNET_ENVIRONMENT",
                    Value = "Development"
                }
            }
        };

        viewModel.HasEnvironment.Should().BeTrue();
        viewModel.EnvironmentSummary.Should().Be("ASPNETCORE_ENVIRONMENT=Development, DOTNET_ENVIRONMENT=Development");
    }

    [Fact]
    public void IsDevelopmentOnly_ReflectsDevelopmentEnvironment()
    {
        var developmentViewModel = new ResourceViewModel
        {
            Environment = new List<AspireEnvironmentEntry>
            {
                new()
                {
                    Name = "ASPNETCORE_ENVIRONMENT",
                    Value = "Development"
                }
            }
        };

        var productionViewModel = new ResourceViewModel
        {
            Environment = new List<AspireEnvironmentEntry>
            {
                new()
                {
                    Name = "ASPNETCORE_ENVIRONMENT",
                    Value = "Production"
                }
            }
        };

        developmentViewModel.IsDevelopmentOnly.Should().BeTrue();
        productionViewModel.IsDevelopmentOnly.Should().BeFalse();
    }

    [Fact]
    public void EnvironmentSummaryText_ShowsDevBadge_ForDevelopmentOnlyResources()
    {
        var viewModel = new ResourceViewModel
        {
            Environment = new List<ElBruno.AspireMonitor.Models.AspireEnvironmentEntry>
            {
                new()
                {
                    Name = "ASPNETCORE_ENVIRONMENT",
                    Value = "Development"
                }
            }
        };

        viewModel.HasEnvironment.Should().BeTrue();
        viewModel.IsDevelopmentOnly.Should().BeTrue();
        viewModel.EnvironmentSummaryText.Should().Be("Env: Dev");
    }

    [Fact]
    public void EnvironmentSummaryText_ShowsCount_ForNonDevelopmentResources()
    {
        var viewModel = new ResourceViewModel
        {
            Environment = new List<ElBruno.AspireMonitor.Models.AspireEnvironmentEntry>
            {
                new()
                {
                    Name = "POSTGRES_PASSWORD",
                    Value = "secret"
                },
                new()
                {
                    Name = "POSTGRES_DB",
                    Value = "app"
                }
            }
        };

        viewModel.HasEnvironment.Should().BeTrue();
        viewModel.IsDevelopmentOnly.Should().BeFalse();
        viewModel.EnvironmentSummaryText.Should().Be("Env: 2 vars");
    }
}
