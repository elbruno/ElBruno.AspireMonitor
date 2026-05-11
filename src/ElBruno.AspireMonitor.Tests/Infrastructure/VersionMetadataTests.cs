using System.Reflection;
using System.Xml.Linq;
using ElBruno.AspireMonitor.Infrastructure;
using ElBruno.AspireMonitor.ViewModels;
using FluentAssertions;
using Xunit;

namespace ElBruno.AspireMonitor.Tests.Infrastructure;

public class VersionMetadataTests
{
    [Fact]
    public void VersionHelper_ReturnsSemanticVersion_FromApplicationAssemblyMetadata()
    {
        VersionHelper.GetAppVersion().Should().MatchRegex(@"^\d+\.\d+\.\d+$");
    }

    [Fact]
    public void MainAndMiniViewModels_ExposeTheSameApplicationVersion()
    {
        var mainVm = new MainViewModel();
        var miniVm = new MiniMonitorViewModel();

        miniVm.AppVersion.Should().Be(mainVm.AppVersion);
        mainVm.AppVersionTitle.Should().Be($"Aspire Monitor {mainVm.AppVersion}");
    }

    [Fact]
    public void ProjectVersionMetadata_StaysAlignedWithAssemblyVersionAndFileVersion()
    {
        var projectPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "ElBruno.AspireMonitor",
            "ElBruno.AspireMonitor.csproj"));
        var project = XDocument.Load(projectPath);
        var properties = project.Root!.Elements("PropertyGroup").Elements().ToDictionary(element => element.Name.LocalName, element => element.Value);

        properties["Version"].Should().MatchRegex(@"^\d+\.\d+\.\d+$");
        properties["AssemblyVersion"].Should().Be($"{properties["Version"]}.0");
        properties["FileVersion"].Should().Be($"{properties["Version"]}.0");

        var assemblyVersion = typeof(VersionHelper).Assembly.GetName().Version;
        assemblyVersion.Should().NotBeNull();
        $"{assemblyVersion!.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}"
            .Should().Be(VersionHelper.GetAppVersion());
    }
}
