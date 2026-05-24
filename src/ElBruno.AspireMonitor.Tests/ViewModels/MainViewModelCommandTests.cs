using ElBruno.AspireMonitor.Models;
using AppConfig = ElBruno.AspireMonitor.Models.Configuration;
using ElBruno.AspireMonitor.Services;
using ElBruno.AspireMonitor.ViewModels;
using FluentAssertions;
using Moq;
using Xunit;
using ElBruno.AspireMonitor.Tests.Services;

namespace ElBruno.AspireMonitor.Tests.ViewModels;

public class MainViewModelCommandTests
{
    [Fact]
    public async Task StartAspireCommand_CallsStartAspireAsync()
    {
        // Arrange
        var workingFolder = Environment.CurrentDirectory;
        var configService = new Mock<IConfigurationService>();
        configService.Setup(service => service.LoadConfiguration())
            .Returns(new AppConfig { ProjectFolder = workingFolder });

        var commandService = new Mock<IAspireCommandService>();
        commandService.Setup(service => service.StartAspireAsync(workingFolder, It.IsAny<Action<string>?>()))
            .ReturnsAsync(false);

        var viewModel = new MainViewModel(null, configService.Object, commandService.Object);

        // Act
        viewModel.StartAspireCommand.Should().NotBeNull();
        viewModel.StartAspireCommand!.Execute(null);

        await WaitUntilAsync(() =>
            commandService.Invocations.Any(invocation => invocation.Method.Name == nameof(IAspireCommandService.StartAspireAsync)));

        // Assert
        commandService.Verify(
            service => service.StartAspireAsync(workingFolder, It.IsAny<Action<string>?>()),
            Times.Once,
            "StartAspireCommand should call IAspireCommandService.StartAspireAsync");
        commandService.Verify(
            service => service.DetectAspireEndpointAsync(It.IsAny<Action<string>?>()),
            Times.Never,
            "endpoint detection only runs after a successful start");
    }

    [Fact]
    public async Task SelectResource_WithLiveLogsService_StreamsLogsIntoLogPanel()
    {
        var configService = new Mock<IConfigurationService>();
        configService.Setup(service => service.LoadConfiguration())
            .Returns(new AppConfig { ProjectFolder = Environment.CurrentDirectory });

        using var liveLogsService = new AspireLiveLogsService(new StreamingAspireCliService("line 1", "line 2"));
        var viewModel = new MainViewModel(null, configService.Object, null, liveLogsService);

        var resource = new ResourceViewModel
        {
            Name = "api",
            Status = ResourceStatus.Running
        };
        viewModel.Resources.Add(resource);

        viewModel.SelectResource(resource);

        await WaitUntilAsync(() => viewModel.LogLines.Any(line => line.Contains("line 2")));

        viewModel.LogLines.Should().Contain(line => line.Contains("Streaming live logs for: api"));
        viewModel.LogLines.Should().Contain("line 1");
        viewModel.LogLines.Should().Contain("line 2");
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        while (!condition())
        {
            timeout.Token.ThrowIfCancellationRequested();
            await Task.Delay(25, timeout.Token);
        }
    }
}
