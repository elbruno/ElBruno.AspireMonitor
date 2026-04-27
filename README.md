# ElBruno.AspireMonitor

[![NuGet](https://img.shields.io/nuget/v/ElBruno.AspireMonitor.svg)](https://www.nuget.org/packages/ElBruno.AspireMonitor/)
[![Downloads](https://img.shields.io/nuget/dt/ElBruno.AspireMonitor.svg)](https://www.nuget.org/packages/ElBruno.AspireMonitor/)
[![Build Status](https://img.shields.io/github/actions/workflow/status/elbruno/ElBruno.AspireMonitor/publish.yml?branch=main)](https://github.com/elbruno/ElBruno.AspireMonitor/actions)
[![.NET](https://img.shields.io/badge/.NET-10-blue)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**A Windows system tray monitor that discovers and displays Aspire running instances and their deployed resources.**

Set a working folder → monitor discovers an active Aspire instance → instantly see all deployed services, containers, and databases with clickable URLs. Zero configuration beyond pointing to your Aspire project directory.

## What It Does

ElBruno.AspireMonitor is a lightweight Windows system tray tool that works in two steps:

1. **Set a working folder** — Point the app to your Aspire AppHost project directory
2. **Auto-discover & monitor** — When `aspire run` is active in that directory, the tray icon turns green and displays:
   - All deployed services, containers, and databases
   - Real-time status and health checks
   - Clickable URLs for each resource
   - CPU/memory metrics for your resources

**The workflow:** Working Folder → Running Instance Detection → Live Resource Display

Perfect for developers who want instant visibility into what Aspire deployed, without switching to a browser dashboard.

## 🎯 Features

| Feature | Description |
|---------|-------------|
| 🖼️ **Tray Status Indicator** | Green (running), Yellow (warning), Red (error), Gray (not running) |
| 🔍 **Automatic Discovery** | Finds running Aspire instances in your working folder |
| 🖥️ **Resource Visibility** | Lists services, containers, databases with endpoints |
| 🔗 **Clickable URLs** | Open any resource directly from the app |
| ⚡ **Real-Time Updates** | Automatic polling every 2 seconds (configurable) |
| 📊 **Status Metrics** | CPU, memory, health status for each resource |
| 🪟 **System Tray Integration** | Minimal footprint, always accessible in taskbar |
| ⚙️ **Single Tray Icon** | One icon for unified monitoring—no clutter |
| 💼 **Working Folder Display** | Shows your chosen directory (humanized paths) |

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

## 🖼️ System Tray Status

The tray icon tells you at a glance:

| Icon | Status | Meaning |
|------|--------|---------|
| <img src="./images/aspire_trayicon_running.png" width="24" alt="Running"> | Running | Aspire instance found; all resources healthy |
| <img src="./images/aspire_trayicon_warning.png" width="24" alt="Warning"> | Warning | Aspire running; one or more resources in warning state |
| <img src="./images/aspire_trayicon_error.png" width="24" alt="Error"> | Error | Lost connection to Aspire or polling failed; auto-reconnect in progress |
| <img src="./images/aspire_trayicon_norunning.png" width="24" alt="Not Running"> | Not Running | No Aspire instance found in the configured working folder |

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

- **Click icon** — Show/hide resource details window
- **Double-click** — Expand/collapse the main window
- **Right-click** — Context menu (Settings, Refresh, Exit)
- **Icon status** — Reflects instance status (green=running, yellow=warning, red=error, gray=not running)

The app automatically watches your working folder for Aspire instances. When `aspire run` is active, resources appear instantly.

### Configuration

Edit your settings at:
```
%APPDATA%\Local\ElBruno\AspireMonitor\config.json
```

Example (working folder, polling interval, thresholds):
```json
{
  "workingFolder": "C:\\Projects\\MyAspireApp",
  "pollingIntervalMs": 2000,
  "cpuThresholdWarning": 70,
  "cpuThresholdCritical": 90,
  "memoryThresholdWarning": 70,
  "memoryThresholdCritical": 90
}
```

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

- **Local Development** — Keep tabs on your microservices while coding
- **Resource Debugging** — Quickly spot which service is consuming CPU/memory
- **Status At-a-Glance** — One tray icon shows everything (no browser switching)
- **Demo Mode** — Impress colleagues with instant visibility into deployed resources
- **Integration Testing** — Monitor health during automated test runs

## 🏗️ Architecture Highlights

- **Service-Oriented**: Clean separation between Aspire API client, discovery, polling, and UI
- **Polling Model**: Configurable background thread with exponential backoff on failures
- **MVVM Pattern**: Fully testable business logic, UI-agnostic services
- **State Machine**: Discrete states (Idle → Discovering → Connected → Polling → Error → Reconnecting)
- **Threshold-Based Alerts**: Configurable CPU/memory thresholds determine status colors
- **Single Tray Icon**: Unified monitoring interface—no confusion from multiple icons

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

**Built with ❤️ for the .NET Aspire community**

Questions? [Open an issue](https://github.com/elbruno/ElBruno.AspireMonitor/issues) or reach out on [GitHub Discussions](https://github.com/elbruno/ElBruno.AspireMonitor/discussions).
