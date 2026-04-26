# Changelog

All notable changes to ElBruno.AspireMonitor will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-04-27

### 🎉 Initial Release

First public release of **ElBruno.AspireMonitor** — Real-time Windows system tray monitor for .NET Aspire distributed applications.

### ✨ Features

#### Core Monitoring
- **Real-Time Polling**: Automatic resource monitoring every 2 seconds (configurable)
- **Color-Coded Status**: 🟢 Green (<70%), 🟡 Yellow (70-90%), 🔴 Red (>90% or error)
- **Multi-Resource Tracking**: Monitor unlimited Aspire resources simultaneously
- **Auto-Reconnect**: Graceful handling of network interruptions with exponential backoff

#### User Interface
- **System Tray Integration**: Minimal, always-available monitoring in Windows taskbar
- **Dynamic Icon**: Tray icon reflects overall system health (green/yellow/red/gray)
- **Resource Details Window**: Click icon to view detailed resource metrics
- **Clickable URLs**: Open resource URLs directly from the application
- **Context Menu**: Right-click access to Settings and Exit

#### Configuration
- **Configurable Thresholds**: Set custom CPU/memory warning and critical points
- **Adjustable Polling Interval**: Customize refresh rate from 1-60 seconds
- **Persistent Settings**: Configuration stored in AppData\Local\ElBruno\AspireMonitor
- **Easy Setup**: First-run wizard for endpoint configuration

#### Technical Architecture
- **.NET 10**: Built on the latest .NET runtime
- **WPF MVVM**: Clean separation of concerns, fully testable
- **Polly Resilience**: Exponential backoff retry policies for API failures
- **State Machine**: Discrete connection states (Idle → Connecting → Polling → Error → Reconnecting)
- **IDisposable Pattern**: Proper resource cleanup and memory management

### 🧪 Quality

- **72 Unit Tests**: 100% passing test suite
- **>80% Code Coverage**: Comprehensive test coverage on Services and Models
- **Edge Case Handling**: Tested against malformed JSON, timeouts, empty data, large datasets
- **Integration Tests**: Validated polling service with mocked API responses

### 📚 Documentation

- **Architecture Guide**: System design, components, data flow
- **Configuration Guide**: Setup, CLI usage, advanced options
- **Development Guide**: Building from source, debugging, contributing
- **Publishing Guide**: NuGet release process with OIDC
- **Troubleshooting Guide**: Common issues and solutions
- **Promotional Templates**: Blog post, LinkedIn, Twitter content

### 🎨 Design Assets

- **NuGet Icon**: 256x256px and 128x128px professional branding
- **Social Media Graphics**: LinkedIn (1200x630), Twitter (1024x512), Blog header
- **Brand Guidelines**: Consistent visual identity across all materials

### 🔧 Technical Details

#### Dependencies
- `Microsoft.Extensions.Http` 9.0.0
- `Polly` 8.5.0

#### Requirements
- Windows 10 or later
- .NET 10 Runtime
- .NET Aspire (running locally or remotely)

### 📦 Installation

```bash
dotnet tool install --global ElBruno.AspireMonitor
```

### 🚀 Usage

```bash
aspire-monitor
```

### 🔗 Links

- **NuGet Package**: https://www.nuget.org/packages/ElBruno.AspireMonitor
- **GitHub Repository**: https://github.com/elbruno/ElBruno.AspireMonitor
- **Documentation**: https://github.com/elbruno/ElBruno.AspireMonitor#readme
- **Issues**: https://github.com/elbruno/ElBruno.AspireMonitor/issues
- **License**: MIT

---

### Development Team

Built by the **Squad** team for this project:
- **Leia** (Lead) — Architecture, code review, release management
- **Luke** (Backend) — API client, polling service, configuration
- **Han** (Frontend) — WPF UI, MVVM ViewModels, system tray integration
- **Yoda** (Testing) — Test infrastructure, 72 comprehensive tests
- **Lando** (Design) — Visual branding, design assets, graphics
- **Chewie** (Documentation) — 8 comprehensive guides + promotional content

---

**Full Changelog**: https://github.com/elbruno/ElBruno.AspireMonitor/commits/v1.0.0
