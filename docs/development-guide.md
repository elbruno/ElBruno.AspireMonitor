# Development Guide

## Prerequisites

Before developing AspireMonitor, install:

- **.NET 10 SDK** ([download](https://dotnet.microsoft.com/en-us/download)) — Required for building and running
- **Windows 10 or later** — WPF is Windows-only
- **Visual Studio 2022 or VS Code** — With C# extension
- **Git** — For version control

### Verify Prerequisites

```bash
# Check .NET version (should be 10.0 or later)
dotnet --version

# Check git
git --version
```

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/elbruno/ElBruno.AspireMonitor.git
cd ElBruno.AspireMonitor
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Project

```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release
```

### 4. Run the Application

```bash
# Run directly
dotnet run --project src/ElBruno.AspireMonitor

# Or use Visual Studio: Press F5
```

## Project Structure

```
ElBruno.AspireMonitor/
├── src/
│   └── ElBruno.AspireMonitor/
│       ├── Views/
│       │   ├── MainWindow.xaml          — Main notification window
│       │   ├── MainWindow.xaml.cs       — WPF code-behind
│       │   └── SettingsWindow.xaml      — Configuration UI
│       ├── ViewModels/
│       │   ├── MainViewModel.cs         — MVVM binding logic
│       │   ├── ResourceViewModel.cs     — Per-resource display model
│       │   └── SettingsViewModel.cs     — Configuration binding
│       ├── Services/
│       │   ├── AspireApiClient.cs       — HTTP API wrapper
│       │   ├── AspirePollingService.cs  — Background polling thread
│       │   ├── StatusCalculator.cs      — Health status logic
│       │   └── ConfigurationService.cs  — Config persistence
│       ├── Models/
│       │   ├── Resource.cs              — Resource data model
│       │   ├── Status.cs                — Status enum
│       │   └── Configuration.cs         — Config data model
│       └── App.xaml
├── src/ElBruno.AspireMonitor.Tests/
│   ├── Services/
│       ├── AspireApiClientTests.cs      — API client tests
│       ├── AspirePollingServiceTests.cs — Polling logic tests
│       ├── StatusCalculatorTests.cs     — Status logic tests
│       └── ConfigurationServiceTests.cs — Config tests
├── docs/                                — Documentation
├── images/                              — Assets and icons
├── build/                               — Build scripts
├── README.md
└── LICENSE
```

## Key Files

### UI Components

- **MainWindow.xaml** — WPF notification window with resource list
- **SettingsWindow.xaml** — Configuration UI for entering endpoint URL and thresholds

### View Models (MVVM)

- **MainViewModel.cs** — Exposes `ObservableCollection<ResourceViewModel>` for binding
- **ResourceViewModel.cs** — Individual resource display (name, CPU, memory, status color)
- **SettingsViewModel.cs** — Configuration form binding

### Services

- **AspireApiClient.cs** — HTTP communication with Aspire API
- **AspirePollingService.cs** — Background thread orchestrating updates
- **StatusCalculator.cs** — CPU/memory threshold evaluation
- **ConfigurationService.cs** — JSON config file persistence

### Models

- **Resource.cs** — Resource data (name, CPU%, memory%, URL)
- **Status.cs** — Enum: `Green`, `Yellow`, `Red`, `Gray`
- **Configuration.cs** — Config properties (endpoint, thresholds, polling interval)

## Building and Testing

### Build Commands

```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release

# Clean before rebuild
dotnet clean && dotnet build
```

### Run Tests

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test -v d

# Run specific test file
dotnet test --filter "ClassName=StatusCalculatorTests"

# Run with code coverage
dotnet test /p:CollectCoverage=true
```

### Run as Global Tool (Local)

```bash
# Pack as .nupkg
dotnet pack -c Release

# Install locally from .nupkg
dotnet tool install --global --add-source ./bin/Release ElBruno.AspireMonitor

# Run the tool
aspire-monitor

# Uninstall local version
dotnet tool uninstall --global ElBruno.AspireMonitor
```

## Development Workflow

### Creating a Feature

1. **Create a feature branch**
   ```bash
   git checkout -b feature/real-time-alerts
   ```

2. **Write tests first (TDD)**
   ```bash
   dotnet test src/ElBruno.AspireMonitor.Tests
   ```

3. **Implement the feature** in Services or ViewModels

4. **Run all tests**
   ```bash
   dotnet test
   ```

5. **Commit with clear message**
   ```bash
   git add .
   git commit -m "feat: add real-time alerts for critical resource usage"
   ```

6. **Push and create Pull Request**
   ```bash
   git push origin feature/real-time-alerts
   ```

### Code Style

Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions):

- Use PascalCase for public properties and methods
- Use camelCase for local variables and parameters
- Use meaningful names (no single letters except loop counters)
- Use XML documentation (`///`) for public APIs
- Avoid excessive comments; code should be self-documenting

Example:
```csharp
/// <summary>
/// Calculates the overall status based on resource metrics.
/// </summary>
/// <param name="cpuUsage">CPU usage percentage (0-100)</param>
/// <param name="memoryUsage">Memory usage percentage (0-100)</param>
/// <returns>Status indicating health (Green, Yellow, Red)</returns>
public Status CalculateStatus(double cpuUsage, double memoryUsage)
{
    // Implementation here
}
```

## Debugging

### Visual Studio 2022

1. **Set breakpoint:** Click in the left margin of any line
2. **Start debugging:** Press `F5` or click Debug → Start Debugging
3. **Step through:** Use F10 (step over) or F11 (step into)
4. **Inspect variables:** Hover over variables or use Watch window

### VS Code with C# DevKit

1. **Install C# extension** from marketplace
2. **Create .vscode/launch.json:**
   ```json
   {
     "version": "0.2.0",
     "configurations": [
       {
         "name": ".NET Launch",
         "type": "coreclr",
         "request": "launch",
         "preLaunchTask": "build",
         "program": "${workspaceFolder}/bin/Debug/net10.0/ElBruno.AspireMonitor.dll",
         "args": [],
         "cwd": "${workspaceFolder}",
         "stopAtEntry": false,
         "console": "integratedTerminal"
       }
     ]
   }
   ```
3. **Start debugging:** Press `F5`

### Common Debug Scenarios

#### Debug API Communication

Add logging to AspireApiClient:
```csharp
Console.WriteLine($"Fetching resources from: {_configuration.AspireEndpoint}/api/resources");
```

#### Debug Status Calculation

Add logging to StatusCalculator:
```csharp
Console.WriteLine($"CPU: {cpuUsage}%, Memory: {memoryUsage}% → Status: {status}");
```

#### Debug Configuration Loading

Check AppData folder:
```bash
# Open configuration file
notepad %APPDATA%\Local\ElBruno\AspireMonitor\config.json
```

#### Monitor Polling Service

Enable debug-level logging:
```csharp
// In ConfigurationService or App.xaml.cs
Debug.WriteLine($"Polling interval: {pollingIntervalMs}ms");
```

## Useful Commands

| Command | Purpose |
|---------|---------|
| `dotnet build` | Build in debug mode |
| `dotnet build -c Release` | Build for release |
| `dotnet test` | Run all tests |
| `dotnet test -v d` | Run tests with verbose output |
| `dotnet run` | Run application |
| `dotnet clean` | Remove build artifacts |
| `dotnet pack` | Create NuGet package |
| `dotnet format` | Auto-format code to standards |
| `dotnet tool install --global --add-source ./bin/Release ElBruno.AspireMonitor` | Install as global tool locally |

## Contributing

1. **Fork the repository** on GitHub
2. **Create a feature branch** (`git checkout -b feature/X`)
3. **Make changes** and write/update tests
4. **Run full test suite** (`dotnet test`)
5. **Commit with clear message** (reference issue if applicable)
6. **Push to your fork** (`git push origin feature/X`)
7. **Create Pull Request** on main repository
8. **Address code review feedback**

### Pull Request Checklist

- [ ] All tests pass (`dotnet test`)
- [ ] Code formatted (`dotnet format` or VS auto-format)
- [ ] Tests added for new features
- [ ] Documentation updated if needed
- [ ] No breaking changes OR version bumped accordingly
- [ ] Commit message follows conventions

## Architecture Reference

For system design, state machine details, and component responsibilities, see [architecture.md](./architecture.md).

## Common Issues

### "dotnet command not found"

**Solution:** Install .NET 10 SDK or add it to PATH:
```bash
# Verify installation
dotnet --version

# Add to PATH if needed (Windows)
set PATH=%PATH%;C:\Program Files\dotnet
```

### "Project file not found"

**Solution:** Ensure you're in the correct directory:
```bash
cd C:\src\ElBruno.AspireMonitor
dir src/*.csproj  # Should list project files
```

### "Tests fail with timeout"

**Solution:** Aspire API may be slow. Increase test timeout:
```bash
dotnet test -- --timeout 30000  # 30 second timeout
```

### "WPF designer not loading in Visual Studio"

**Solution:** Rebuild solution:
```bash
dotnet clean
dotnet build
```

Then close and reopen Visual Studio.

## Additional Resources

- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [WPF Documentation](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
- [Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [MVVM Pattern Guide](https://learn.microsoft.com/en-us/archive/msdn-magazine/2009/february/patterns-wpf-apps-with-the-model-view-viewmodel-design-pattern)
- [xUnit Testing Framework](https://xunit.net/)

---

*Questions? Open an issue on GitHub or check the [troubleshooting.md](./troubleshooting.md) guide.*
