# ElBruno.AspireMonitor

[![NuGet](https://img.shields.io/nuget/v/ElBruno.AspireMonitor.svg)](https://www.nuget.org/packages/ElBruno.AspireMonitor/)
[![Downloads](https://img.shields.io/nuget/dt/ElBruno.AspireMonitor.svg)](https://www.nuget.org/packages/ElBruno.AspireMonitor/)
[![Build Status](https://img.shields.io/github/actions/workflow/status/elbruno/ElBruno.AspireMonitor/publish.yml?branch=main)](https://github.com/elbruno/ElBruno.AspireMonitor/actions)
[![.NET](https://img.shields.io/badge/.NET-10-blue)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**A Windows system tray monitor for Aspire distributed applications.**

Point it at your Aspire AppHost folder, pin the resources you care about, and get a compact mini window with live URLs and Start/Stop controls — without leaving the tray.

## What It Does

ElBruno.AspireMonitor is a lightweight Windows tray tool that:

1. **Watches a working folder** — point it at the directory containing your Aspire `*.AppHost.csproj`
2. **Discovers resources** — shells out to `aspire describe --format json` and parses the result
3. **Surfaces what matters** — lists every resource in the main window, and pins your chosen ones (with their real URLs) in a compact mini window
4. **Drives your AppHost** — Start / Stop buttons run `aspire run` and shut it down cleanly, with a live countdown while Aspire spins up

No third-party Aspire SDK dependency. No agents. Just a tray app talking to the Aspire CLI.

## 🎯 Features

| Feature | Description |
|---------|-------------|
| 🪟 **Tray + main window + mini window** | Click the tray icon for the full resource list; the mini window pins just what you care about |
| 📌 **Pinned resources** | Comma-separated `MiniWindowResources` setting pins resources by name prefix (case-insensitive — `web` matches Aspire's `web-xggqzmyn` replicas) |
| 🔗 **Full URLs inline** | Pinned resources show their actual endpoint (`http://localhost:5021`), not a generic "Open" link |
| ▶️ **Start / Stop controls** | Run or stop your AppHost from the tray. Start stays disabled with a live countdown (`⏳ Starting Aspire... (12 / 90s)`) until resources actually appear |
| 🛡 **Pinned-resource validation** | Missing pins are skipped, not crashes — the mini window opens with whatever resolved |
| ⚙️ **Live config reload** | Changes in Settings apply without restarting the app |
| 🔄 **Configurable polling** | Default 2 second interval, override in config |

## ⚡ Quick Start

1. **Install** as a .NET global tool: `dotnet tool install --global ElBruno.AspireMonitor`
2. **Launch** with `aspiremon` from any terminal
3. **Set working folder** when prompted (point to your Aspire AppHost directory)
4. **Start monitoring** — Run `aspire run` from that directory; the tray icon will turn green with resources listed

**Install as a .NET Global Tool** (recommended):

```bash
dotnet tool install --global ElBruno.AspireMonitor
```

Then launch anytime from any terminal:

```bash
aspiremon
```

The tool is Windows-only (the underlying app is WPF). Requires the [.NET 10 Runtime](https://dotnet.microsoft.com/en-us/download).

**Or download the executable** from [GitHub Releases](https://github.com/elbruno/ElBruno.AspireMonitor/releases/latest).

For detailed setup instructions, see [Quick Start Guide](./docs/QUICKSTART.md).

## 📋 Requirements

- **Windows 10 or later** (WPF is Windows-only)
- **.NET 10 Runtime** ([download](https://dotnet.microsoft.com/en-us/download))
- **.NET Aspire** running locally on your machine

## 🚀 Usage

### First Run

Install the global tool and launch it:

```bash
dotnet tool install --global ElBruno.AspireMonitor
aspiremon
```

On first run, you'll be prompted for your working folder:
```
Enter working folder (path to your Aspire AppHost project):
C:\Projects\MyAspireApp
```

### System Tray

- **Click icon** — show/hide the main window
- **Double-click** — toggle the main window
- **Right-click** — context menu (Settings, Mini window, Exit)

The app watches your working folder. When `aspire run` is active, resources appear automatically.

### Configuration

Open the in-app **Settings** dialog, or edit:
```
%APPDATA%\Local\ElBruno\AspireMonitor\config.json
```

Example:
```json
{
  "WorkingFolder": "C:\\Projects\\MyApp\\src\\MyApp.AppHost",
  "AspireHostUrl": "http://localhost:18888",
  "PollingIntervalMs": 2000,
  "MiniWindowResources": "web, store, gateway"
}
```

| Field | Purpose |
|---|---|
| `WorkingFolder` | Folder containing your Aspire `*.AppHost.csproj` |
| `AspireHostUrl` | Aspire dashboard URL (default `http://localhost:18888`) |
| `PollingIntervalMs` | Resource refresh interval (default 2000) |
| `MiniWindowResources` | Comma-separated list of resource name prefixes to pin to the mini window. Empty = mini window only shows the dashboard link. Case-insensitive prefix match (e.g. `web` matches `web-xggqzmyn`) |

See [Configuration Guide](./docs/configuration.md) for all options.

## 📚 Documentation

- **[Quick Start Guide](./docs/QUICKSTART.md)** — Get up and running in 5 minutes
- **[Architecture Guide](./docs/architecture.md)** — System design, components, data flow
- **[API Contract & Services](./docs/API-CONTRACT.md)** — Service layer, data contracts, retry logic
- **[Configuration Guide](./docs/configuration.md)** — Setup, CLI, advanced options
- **[Development Guide](./docs/development-guide.md)** — Building from source, debugging
- **[Publishing Guide](./docs/publishing.md)** — NuGet publishing, versioning, releases
- **[Troubleshooting Guide](./docs/troubleshooting.md)** — Common issues and solutions

## 💡 Use Cases

- **Local Development** — keep your AppHost one click away while coding
- **Quick Status Checks** — pin the two or three resources you actually hit and see their live URLs without opening the dashboard
- **Demos** — Start / Stop from the tray during walkthroughs
- **Multi-window setups** — the mini window stays out of the way and resizes itself

## 🏗️ Architecture Highlights

- **Service-Oriented** — `AspireCliService` (process I/O) → `AspirePollingService` (interval) → `MainViewModel` → views
- **MVVM** — testable view models, thin XAML, all business logic covered by xUnit + Moq
- **Live config reload** — settings changes flow through without restarting
- **Pinned-resource matcher** — case-insensitive prefix match against `aspire describe` output, with graceful handling of missing pins

See [Architecture Guide](./docs/architecture.md) for detailed design decisions and data flows.

## 📦 Installation & Updates

### Install

```bash
dotnet tool install --global ElBruno.AspireMonitor
```

Then run `aspiremon` from any terminal.

### Update

```bash
dotnet tool update --global ElBruno.AspireMonitor
```

### Uninstall

```bash
dotnet tool uninstall --global ElBruno.AspireMonitor
```

### Verify Version

```bash
dotnet tool list --global
```

## 🐛 Troubleshooting

### Aspire instance not found?

1. Verify Aspire is running: `aspire run` in your working folder
2. Check the working folder setting in config: `%APPDATA%\Local\ElBruno\AspireMonitor\config.json`
3. See [Troubleshooting Guide](./docs/troubleshooting.md) for more solutions

### Tray icon not visible?

- Check Windows notification area (click ▲ in system tray)
- Verify AspireMonitor is running: `tasklist | findstr aspire`
- Restart the application and check tray icon placement
- See [Troubleshooting Guide](./docs/troubleshooting.md)

## 🤝 Contributing

Contributions welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Make changes and write/update tests
4. Run full test suite: `dotnet test`
5. Commit with clear messages
6. Open a Pull Request

See [Development Guide](./docs/development-guide.md) for setup and workflow.

## 📄 License

MIT License — See [LICENSE](./LICENSE) file for details.

## 👋 About the Author

**Bruno Capuano** [@elbruno](https://github.com/elbruno)  
Microsoft AI MVP | GitHub Star

- **Blog:** [elbruno.com](https://elbruno.com)
- **GitHub:** [@elbruno](https://github.com/elbruno)
- **LinkedIn:** [/in/elbruno](https://linkedin.com/in/elbruno)
- **Twitter:** [@elbruno](https://twitter.com/elbruno)
- **YouTube:** [@elbruno](https://youtube.com/@elbruno)

---

**Built with ❤️ for the Aspire community**

Questions? [Open an issue](https://github.com/elbruno/ElBruno.AspireMonitor/issues) or reach out on [GitHub Discussions](https://github.com/elbruno/ElBruno.AspireMonitor/discussions).
