# ElBruno.AspireMonitor

[![NuGet](https://img.shields.io/nuget/v/ElBruno.AspireMonitor.svg)](https://www.nuget.org/packages/ElBruno.AspireMonitor/)
[![Downloads](https://img.shields.io/nuget/dt/ElBruno.AspireMonitor.svg)](https://www.nuget.org/packages/ElBruno.AspireMonitor/)
[![Build Status](https://img.shields.io/github/actions/workflow/status/elbruno/ElBruno.AspireMonitor/publish.yml?branch=main)](https://github.com/elbruno/ElBruno.AspireMonitor/actions)
[![.NET](https://img.shields.io/badge/.NET-10-blue)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**A Windows system tray monitor for Aspire distributed applications.**

Point it at your Aspire AppHost folder, pin the resources you care about, and get CPU, memory, disk, resource type, environment badges, endpoint counts, live URLs, and Start/Stop controls from the tray.

## What It Does

ElBruno.AspireMonitor is a lightweight Windows tray tool that:

1. **Watches a working folder** — point it at your Aspire AppHost folder
2. **Discovers resources** — shells out to `aspire describe --format json` and parses the result
3. **Surfaces what matters** — lists every resource in the main window, and pins your chosen ones (with their real URLs) in a compact mini window
4. **Drives your AppHost** — Start / Stop buttons run `aspire start` and shut it down cleanly, with a live countdown while Aspire spins up

No third-party Aspire SDK dependency. No agents. Just a tray app talking to the Aspire CLI and opening the Aspire dashboard at the standard default `http://localhost:18888`.

## ✨ What's New in v1.11.0

ElBruno.AspireMonitor v1.11.0 adds worktree-aware monitoring for parallel Aspire development:

- **Git Worktree Discovery Mode** — Detects candidate sessions from `git worktree list --porcelain` under a base folder.
- **Aspire Config Validation** — Only monitors worktrees with a valid `aspire.config.json` and `appHost.path`.
- **Unified Multi-Session View** — Aggregates resources across running worktrees and prefixes names with the worktree folder for clarity.

See [CHANGELOG.md](./CHANGELOG.md) for detailed release history.

## 🎯 Features

| Feature | Description |
|---------|-------------|
| 🔔 **State-Change Notifications** | Windows notifications when Aspire starts or stops with clickable dashboard access (configurable via Settings toggle, enabled by default) |
| 🟢🟡🔴 **Color-Coded Status** | Visual indicators: Green (<70%), Yellow (70-90%), Red (>90%) |
| ⚡ **Real-Time Updates** | Automatic polling every 5 seconds (configurable) |
| 📦 **Rich Resource Telemetry** | See type, disk usage percentage, and endpoint counts at a glance |
| 🏷️ **Environment Badges** | Compact badges summarize resource environment variables |
| 🙈 **Hide Development Resources** | Optionally filter development-only resources from the list |
| 🧭 **Open Dashboard** | One-click button opens the configured Aspire dashboard URL |
| 🌲 **Worktree Discovery** | Optionally discover and monitor Aspire sessions running in git worktrees under a base path |
| 🔄 **Auto-Reconnect** | Gracefully handles network interruptions |
| 📊 **Multi-Resource Monitoring** | Track unlimited Aspire resources |
| 🪟 **Tray + main window + mini window** | Click the tray icon for the full resource list; the mini window pins just what you care about |
| 📌 **Pinned resources** | Comma-separated `MiniWindowResources` setting pins resources by name prefix (case-insensitive — `web` matches Aspire's `web-xggqzmyn` replicas) |
| 🔗 **Full URLs inline** | Pinned resources show their actual endpoint (`http://localhost:5021`), not a generic "Open" link |
| ▶️ **Start / Stop controls** | Run or stop your AppHost from the tray. Start stays disabled with a live countdown (`⏳ Starting Aspire... (12 / 90s)`) until resources actually appear |
| 🛡 **Pinned-resource validation** | Missing pins are skipped, not crashes — the mini window opens with whatever resolved |
| ⚙️ **Live config reload** | Changes in Settings apply without restarting the app |
| 🔄 **Configurable polling** | Default 5 second interval, override in config |

## ⚡ Quick Start

1. **Install** as a .NET global tool: `dotnet tool install --global ElBruno.AspireMonitor`
2. **Launch** with `aspiremon` from any terminal
3. **Set working folder** when prompted (point to your Aspire AppHost folder)
4. **Start monitoring** — Use the Start button in the tray to launch Aspire; the tray icon will turn green with resources listed

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
- **Aspire** running locally on your machine

## 🚀 Usage

### First Run

Install the global tool and launch it:

```bash
dotnet tool install --global ElBruno.AspireMonitor
aspiremon
```

On first run, you'll be prompted for your working folder:
```
Enter working folder (path to your Aspire AppHost folder):
C:\Projects\MyAspireApp
```

### System Tray

- **Click icon** — show/hide the main window
- **Double-click** — toggle the main window
- **Right-click** — context menu (Settings, Mini window, Exit)

The app watches your working folder. When Aspire is running, resources appear automatically.

### Configuration

Open the in-app **Settings** dialog, or edit:
```
%APPDATA%\Local\ElBruno\AspireMonitor\config.json
```

Example:
```json
{
  "projectFolder": "C:\\Projects\\MyApp\\src\\MyApp.AppHost",
  "enableWorktreeDiscovery": true,
  "worktreeBasePath": "C:\\Projects\\MyRepo",
  "aspireEndpoint": "http://localhost:18888",
  "pollingIntervalMs": 5000,
  "miniWindowResources": "web, store, gateway",
  "hideDevelopmentResources": false,
  "notifyOnStateChange": true
}
```

| Field | Purpose |
|---|---|
| `projectFolder` | Your Aspire AppHost folder |
| `enableWorktreeDiscovery` | Enables discovery across git worktrees using `aspire.config.json` |
| `worktreeBasePath` | Optional root folder where worktrees are searched (must exist if set) |
| `aspireEndpoint` | Aspire dashboard URL (default `http://localhost:18888`) |
| `pollingIntervalMs` | Resource refresh interval (default 5000) |
| `miniWindowResources` | Comma-separated list of resource name prefixes to pin to the mini window. Empty = mini window only shows the dashboard link. Case-insensitive prefix match (e.g. `web` matches `web-xggqzmyn`) |
| `hideDevelopmentResources` | Hides resources marked as development-only in Aspire environment metadata |
| `notifyOnStateChange` | Windows notification when Aspire state changes running ↔ not running (default true) |

See [Configuration Guide](./docs/configuration.md) for all options.

## 📚 Documentation

- **[Quick Start Guide](./docs/QUICKSTART.md)** — Get up and running in 5 minutes
- **[What's New in v1.11.0](./docs/releases/RELEASE-v1.11.0.md)** — Worktree discovery, aspire.config validation, and multi-session aggregation
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

1. Start Aspire using the Start button in the tray, or run `aspire start` in your working folder
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
