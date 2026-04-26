# ElBruno.AspireMonitor

[![NuGet](https://img.shields.io/nuget/v/ElBruno.AspireMonitor.svg)](https://www.nuget.org/packages/ElBruno.AspireMonitor/)
[![Downloads](https://img.shields.io/nuget/dt/ElBruno.AspireMonitor.svg)](https://www.nuget.org/packages/ElBruno.AspireMonitor/)
[![Build Status](https://img.shields.io/github/actions/workflow/status/elbruno/ElBruno.AspireMonitor/publish.yml?branch=main)](https://github.com/elbruno/ElBruno.AspireMonitor/actions)
[![.NET](https://img.shields.io/badge/.NET-10-blue)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Real-time Windows system tray monitor for .NET Aspire distributed applications.**

Monitor CPU, memory, and health status of your Aspire resources without leaving your code editor. Get instant visual feedback on resource utilization with color-coded status indicators and clickable URLs—all from a lightweight tray icon.

## ⚡ Quick Start

1. **Download** from [GitHub Releases](https://github.com/elbruno/ElBruno.AspireMonitor/releases/latest)
2. **Run** `ElBruno.AspireMonitor.exe`
3. **Enter** your Aspire endpoint (e.g., `http://localhost:5000`)
4. **Monitor** resources in real-time from your system tray

**Or install as a .NET Global Tool:**
```bash
dotnet tool install --global ElBruno.AspireMonitor
aspire-monitor
```

For detailed setup instructions, see [Quick Start Guide](./docs/QUICKSTART.md).

## 🎯 Features

| Feature | Description |
|---------|-------------|
| 🟢🟡🔴 **Color-Coded Status** | Visual indicators: Green (<70%), Yellow (70-90%), Red (>90%) |
| ⚡ **Real-Time Updates** | Automatic polling every 2 seconds (configurable) |
| 🪟 **System Tray Integration** | Minimal, always-available monitoring in your taskbar |
| 🔗 **Clickable URLs** | Open resources directly from the app |
| ⚙️ **Configurable Thresholds** | Set CPU/memory warning and critical points |
| 🔄 **Auto-Reconnect** | Gracefully handles network interruptions |
| 📊 **Multi-Resource Monitoring** | Track unlimited Aspire resources |

## 📋 Requirements

- **Windows 10 or later** (WPF is Windows-only)
- **.NET 10 Runtime** ([download](https://dotnet.microsoft.com/en-us/download))
- **.NET Aspire** installed and running locally or remotely

## 🚀 Usage

### First Run

Download the latest release from [GitHub Releases](https://github.com/elbruno/ElBruno.AspireMonitor/releases/latest) and run `ElBruno.AspireMonitor.exe`.

On first run, you'll be prompted for your Aspire endpoint URL:
```
Enter Aspire endpoint (e.g., http://localhost:5000): http://localhost:5000
```

### System Tray

- **Click icon**: Show/hide resource details
- **Double-click**: Expand/collapse window
- **Right-click**: Context menu (Settings, Exit)
- **Color changes**: Reflects highest resource utilization

### Configuration

Edit configuration at:
```
%APPDATA%\Local\ElBruno\AspireMonitor\config.json
```

Example:
```json
{
  "aspireEndpoint": "http://localhost:5000",
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

- **Local Development** — Monitor microservices while coding
- **Performance Testing** — Watch resource consumption during load tests
- **Debugging** — Quickly identify which resource is problematic
- **Teaching** — Demonstrate resource usage concepts to teams
- **Integration Testing** — Monitor health during automated test runs

## 🏗️ Technology Stack

- **.NET 10** — Modern, performant runtime
- **WPF** — Native Windows UI framework
- **MVVM** — Clean architecture pattern
- **xUnit + Moq** — Comprehensive test coverage
- **MIT License** — Open source, permissive

## 🔍 Architecture Highlights

- **Service-Oriented**: Clean separation between API client, polling service, status calculation, and UI
- **Polling Model**: Configurable background thread with exponential backoff on failures
- **MVVM Pattern**: Fully testable business logic, UI-agnostic services
- **State Machine**: Discrete connection states (Connecting → Connected → Polling → Error → Reconnecting)
- **Threshold-Based Alerts**: Configurable CPU/memory thresholds determine status color

See [Architecture Guide](./docs/architecture.md) for detailed design decisions and component interactions.

## 📦 Installation & Updates

### Download

Download the latest release from [GitHub Releases](https://github.com/elbruno/ElBruno.AspireMonitor/releases/latest):

1. Go to [Releases](https://github.com/elbruno/ElBruno.AspireMonitor/releases)
2. Download `ElBruno.AspireMonitor-v1.0.0.zip`
3. Extract to a folder (e.g., `C:\Tools\AspireMonitor`)
4. Run `ElBruno.AspireMonitor.exe`

### Update

Download the latest version and replace the existing files.

### Uninstall

Simply delete the folder containing the application.

### Verify Version

Right-click `ElBruno.AspireMonitor.exe` → **Properties** → **Details** tab

## 🐛 Troubleshooting

### Can't connect to Aspire?

1. Verify Aspire is running: `http://localhost:5000`
2. Check configuration file: `%APPDATA%\Local\ElBruno\AspireMonitor\config.json`
3. See [Troubleshooting Guide](./docs/troubleshooting.md) for more solutions

### Tray icon not visible?

- Check Windows notification area (click ▲ in system tray)
- Verify AspireMonitor is running: `tasklist | findstr aspire`
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
