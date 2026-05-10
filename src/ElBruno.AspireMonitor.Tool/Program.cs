// aspiremon — .NET global tool launcher for ElBruno.AspireMonitor (WPF tray app).
// The WPF payload is bundled into the `desktop/` folder of the tool package
// by build/Inject-DesktopPayload.ps1 during pack.

return LaunchDesktopApp(args);

static int LaunchDesktopApp(string[] args)
{
    if (!OperatingSystem.IsWindows())
    {
        Console.Error.WriteLine("aspiremon requires Windows (the underlying app is a WPF system tray monitor).");
        return 1;
    }

    try
    {
        var desktopDirectory = Path.Combine(AppContext.BaseDirectory, "desktop");
        var desktopExecutablePath = Path.Combine(desktopDirectory, "ElBruno.AspireMonitor.exe");

        if (!File.Exists(desktopExecutablePath))
        {
            Console.Error.WriteLine($"Desktop app payload is missing from the tool installation. Expected: {desktopExecutablePath}");
            Console.Error.WriteLine("Try reinstalling: dotnet tool update --global ElBruno.AspireMonitor");
            return 1;
        }

        var psi = new ProcessStartInfo
        {
            FileName = desktopExecutablePath,
            WorkingDirectory = desktopDirectory,
            UseShellExecute = true,
        };
        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        using var process = Process.Start(psi);
        return process is null ? 1 : 0;
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"Unable to launch ElBruno.AspireMonitor: {exception.Message}");
        return 1;
    }
}
