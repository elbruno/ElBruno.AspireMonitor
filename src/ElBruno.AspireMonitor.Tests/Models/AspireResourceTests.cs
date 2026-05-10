using System.Text.Json;
using FluentAssertions;
using ElBruno.AspireMonitor.Models;
using Xunit;

namespace ElBruno.AspireMonitor.Tests.Models;

public class AspireResourceTests
{
    [Fact]
    public async Task DeserializeTelemetryRichFixture_MapsEnvironmentArray()
    {
        var json = await File.ReadAllTextAsync(Path.Combine("Fixtures", "aspire-response-telemetry-rich.json"));
        using var document = JsonDocument.Parse(json);

        var resources = JsonSerializer.Deserialize<List<AspireResource>>(
            document.RootElement.GetProperty("resources").GetRawText(),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        resources.Should().NotBeNull();
        resources.Should().HaveCount(2);

        var developmentResource = resources![0];
        developmentResource.Environment.Should().HaveCount(2);
        developmentResource.Environment[0].Name.Should().Be("ASPNETCORE_ENVIRONMENT");
        developmentResource.Environment[0].Value.Should().Be("Development");
        developmentResource.IsDevelopmentOnly.Should().BeTrue();

        var productionResource = resources[1];
        productionResource.Environment.Should().HaveCount(1);
        productionResource.Environment[0].Value.Should().Be("Production");
        productionResource.IsDevelopmentOnly.Should().BeFalse();
    }
}
