# ElBruno.AspireMonitor — Quick Start Guide

Get monitoring your Aspire resources in 5 minutes.

## ⚡ Prerequisites

Before starting, ensure you have:

- **Windows 10 or later** — WPF is Windows-only
- **.NET 10 Runtime** — [Download](https://dotnet.microsoft.com/en-us/download)
- **.NET Aspire running** — Locally or remotely (e.g., `http://localhost:5000`)

Verify your setup:
```bash
dotnet --version              # Should be 10.x or higher
dotnet workload list          # Should include 'aspire'
```

## 🚀 Installation

### Option 1: Download Pre-Built Executable (Easiest)

1. Go to [GitHub Releases](https://github.com/elbruno/ElBruno.AspireMonitor/releases/latest)
2. Download `ElBruno.AspireMonitor.exe` for your Windows version
3. Extract the ZIP file
4. Double-click `ElBruno.AspireMonitor.exe`

### Option 2: Install as .NET Global Tool

```bash
dotnet tool install --global ElBruno.AspireMonitor
```

Then run:
```bash
aspire-monitor
```

### Option 3: Build from Source

Clone the repository:
```bash
git clone https://github.com/elbruno/ElBruno.AspireMonitor.git
cd ElBruno.AspireMonitor
```

Build the project:
```bash
dotnet build -c Release
cd src/ElBruno.AspireMonitor
dotnet run
```

## ⚙️ Configuration

### First Run Setup

When you launch AspireMonitor for the first time, it will prompt you:

```
Enter Aspire endpoint (e.g., http://localhost:5000): 
```

**Common endpoints:**
- **Local development**: `http://localhost:5000`
- **Remote server**: `http://192.168.1.100:5000`
- **HTTPS**: `https://aspire.example.com`

Press **Enter** to save. The application will start monitoring immediately.

### Configuration File

All settings are stored at:
```
%APPDATA%\Local\ElBruno\AspireMonitor\config.json
```

Example configuration:
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

**Edit settings directly** by opening the config file in your text editor (requires app restart to apply).

### Common Configuration Scenarios

#### Fast Polling (More Responsive)
```json
{
  "pollingIntervalMs": 1000
}
```

#### Sensitive Warnings
```json
{
  "cpuThresholdWarning": 50,
  "memoryThresholdWarning": 60
}
```

#### Remote Server (HTTPS)
```json
{
  "aspireEndpoint": "https://aspire-prod.example.com"
}
```

See [Configuration Guide](./configuration.md) for all options.

## 📊 Understanding the UI

### Main Window

When you click the tray icon, you'll see:

```
📊 Resource Status Monitor
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
│ Resource Name         │ CPU   │ Memory │ Status
├───────────────────────┼───────┼────────┼────────
│ api-service     🟢     │ 35%   │ 245MB  │ Running
│ db-postgres     🟡     │ 72%   │ 512MB  │ Running
│ cache-redis     🟢     │ 42%   │ 128MB  │ Running
│ worker-queue    🔴     │ 95%   │ 876MB  │ Warning
└───────────────────────┴───────┴────────┴────────
[🔄 Refresh]  [⚙️ Settings]  [❌ Exit]
```

**Color meanings:**
- 🟢 **Green** — All resources <70% utilization
- 🟡 **Yellow** — Resource 70-90% utilization
- 🔴 **Red** — Resource >90% or connection error

### System Tray Icon

The tray icon shows the **highest status** across all resources:

| Icon | Meaning | Action |
|------|---------|--------|
| 🟢 | All resources healthy | Everything is fine |
| 🟡 | Some resources warning | Check CPU/memory usage |
| 🔴 | Critical or error | Investigate immediately |
| ⚪ | Disconnected | Check Aspire endpoint |

**Tray interactions:**
- **Left-click** — Show/hide window
- **Double-click** — Expand/collapse
- **Right-click** — Context menu (Settings, Exit)

## 🔗 Click to Open Resources

Resources in the main window are **clickable URLs**. Click any resource name to open it in your browser:

- Clicking `api-service` → Opens `http://localhost:5000/resources/api-service`
- Clicking `db-postgres` → Opens `http://localhost:5000/resources/db-postgres`

## 🔄 Live Polling in Action

AspireMonitor continuously monitors your Aspire resources:

1. **Connects** to your Aspire API (default: every 2 seconds)
2. **Fetches** resource metrics (CPU, memory, status)
3. **Evaluates** health against thresholds
4. **Updates** tray icon and window in real-time
5. **Auto-reconnects** if connection drops

**Graceful offline handling:**
- Network timeout? Shows gray/disconnected state
- Aspire restarts? Automatically reconnects
- Manual pause? Right-click > Settings > Pause polling

## 🛠️ Troubleshooting

### "Can't connect to Aspire"

**Check:**
1. Is Aspire running? `dotnet run` in your Aspire project directory
2. Is the endpoint correct? Default is usually `http://localhost:5000`
3. Is the port accessible? Check Windows Firewall settings

**Solution:**
```bash
# Edit configuration
notepad %APPDATA%\Local\ElBruno\AspireMonitor\config.json
# Verify "aspireEndpoint" matches your running Aspire instance
```

### Tray Icon Won't Update

**Check:**
1. Is polling interval too long? (default 2000ms is fine)
2. Are thresholds unrealistic? (e.g., warning at 100%)
3. Is CPU/memory actually available?

**Solution:**
```json
{
  "pollingIntervalMs": 2000,
  "cpuThresholdWarning": 70,
  "memoryThresholdWarning": 70
}
```

### High CPU/Memory Usage

AspireMonitor itself should use <50MB memory and <1% CPU when idle.

**If high:**
1. Increase polling interval: `"pollingIntervalMs": 5000` (poll every 5s)
2. Check Aspire API isn't returning huge datasets
3. Restart the application

See [Troubleshooting Guide](./troubleshooting.md) for more issues.

## 📚 Next Steps

- **Want to customize?** → [Configuration Guide](./configuration.md)
- **Curious about architecture?** → [Architecture Guide](./architecture.md)
- **Building from source?** → [Development Guide](./development-guide.md)
- **Troubleshooting?** → [Troubleshooting Guide](./troubleshooting.md)

## 🎯 Common Use Cases

### Local Development
Monitor microservices while coding:
```json
{
  "aspireEndpoint": "http://localhost:5000",
  "pollingIntervalMs": 2000
}
```

### Load Testing
Watch resource consumption during tests:
```json
{
  "pollingIntervalMs": 1000,
  "cpuThresholdWarning": 60
}
```

### Remote Server Monitoring
Monitor production Aspire deployment:
```json
{
  "aspireEndpoint": "https://aspire-prod.example.com",
  "pollingIntervalMs": 5000
}
```

## 🔗 Resources

- **GitHub**: https://github.com/elbruno/ElBruno.AspireMonitor
- **NuGet Package**: https://www.nuget.org/packages/ElBruno.AspireMonitor
- **Issues & Feedback**: https://github.com/elbruno/ElBruno.AspireMonitor/issues
- **License**: MIT

---

**Happy monitoring!** 🚀

For detailed documentation, see [README.md](../README.md).
