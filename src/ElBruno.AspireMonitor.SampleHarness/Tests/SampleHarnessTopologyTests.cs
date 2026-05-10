using System.Reflection;

namespace ElBruno.AspireMonitor.SampleHarness.Tests;

public class SampleHarnessTopologyTests
{
    [Fact]
    public void AppHost_declares_all_sample_services()
    {
        var appHost = GetSourceFile("AppHost", "AppHost.cs");

        Assert.Contains("catalog-api", appHost);
        Assert.Contains("orders-api", appHost);
        Assert.Contains("worker", appHost);
    }

    private static string GetSourceFile(params string[] segments)
    {
        var harnessRoot = Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "..",
            "..",
            "..",
            ".."));

        return File.ReadAllText(Path.Combine(harnessRoot, Path.Combine(segments)));
    }
}
