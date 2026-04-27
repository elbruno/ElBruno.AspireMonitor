# Blog Post: Introducing AspireMonitor

## Title

**Introducing AspireMonitor: Real-Time Aspire Monitoring in Your System Tray**

---

## Headline

For developers building distributed applications with .NET Aspire, monitoring resource health is critical—but checking dashboards constantly breaks focus. **AspireMonitor** puts real-time Aspire metrics directly in your Windows system tray, giving you instant visibility into CPU, memory, and status of all your Aspire resources without leaving your code editor.

---

## Why This Matters

.NET Aspire has revolutionized distributed application development by giving developers a unified, intuitive way to orchestrate microservices, databases, and caches locally. But Aspire's excellent dashboard lives in a browser tab—one more thing to context-switch to when you need to check if your API is eating memory or your database is saturated.

**AspireMonitor solves this problem.** A lightweight Windows system tray icon gives you real-time status at a glance:
- 🟢 **Green**: All resources healthy (<70% CPU/memory)
- 🟡 **Yellow**: Warning zone (70-90% utilization)
- 🔴 **Red**: Critical (>90% or connection error)

---

## Key Features

### ⚡ Real-Time Monitoring

AspireMonitor polls your Aspire dashboard every 2 seconds (configurable) and updates instantly. No manual refreshes. No lag. Just live data.

### 🚦 Color-Coded Status

At a glance, know if your infrastructure is healthy:
- Green icon? All systems nominal.
- Yellow icon? Time to check what's consuming resources.
- Red icon? Something needs immediate attention.

### 🔗 Clickable URLs

Right-click any resource in the expanded view and open it directly—no copy/paste, no hunting through logs.

### ⚙️ Configurable Thresholds

Default thresholds work for most, but your app is unique. Set CPU/memory warning and critical points in one simple JSON file:

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

### 🪟 System Tray Integration

Minimize it, forget about it. AspireMonitor runs in the background, never interrupting your workflow. Click the icon anytime to see resource details. Double-click to expand/collapse.

### 🔄 Auto-Reconnect

Network hiccup? Aspire restarted? AspireMonitor automatically reconnects with exponential backoff—no manual intervention needed.

---

## Getting Started

### Installation

```bash
dotnet tool install --global ElBruno.AspireMonitor
```

### Run It

```bash
aspiremon
```

That's it. The first time you run it, you'll be prompted for your working folder (point to your Aspire AppHost project directory).

### Configure (Optional)

Edit `%APPDATA%\Local\ElBruno\AspireMonitor\config.json` to adjust polling interval or thresholds. Restart the app and you're done.

---

## Perfect For

- **Local Development**: Keep tabs on your microservices while coding
- **Performance Testing**: Watch resource consumption in real-time during load tests
- **Debugging**: Quickly spot which resource is misbehaving
- **Teaching**: Show students what happens to resource usage under load
- **Integration Testing**: Run tests while monitoring system health

---

## How It Works (Architecture)

AspireMonitor uses a simple, elegant architecture:

1. **AspireApiClient** — Talks to Aspire's HTTP API
2. **AspirePollingService** — Runs background thread at configurable interval
3. **StatusCalculator** — Evaluates CPU/memory against thresholds
4. **MainViewModel** — MVVM binding to WPF
5. **WPF UI** — Lightweight notification window + system tray

No external dependencies. No bloat. Just you, your Aspire apps, and one small green icon. Or yellow. Or red.

---

## Technology Stack

- **.NET 10** — Modern, performant runtime
- **WPF** — Native Windows integration
- **MVVM** — Clean architecture for testability
- **MIT License** — Open source, do what you want with it

---

## Open Source & Extensible

AspireMonitor is on GitHub with MIT license. Want to add features? Contribute. Want to run it on macOS? Fork it and port it. The codebase is small, clean, and well-documented.

**GitHub:** [github.com/elbruno/ElBruno.AspireMonitor](https://github.com/elbruno/ElBruno.AspireMonitor)

---

## What's Next?

v1.3.0 ships as a .NET global tool, making installation and updates seamless via `dotnet tool` commands. Future roadmap includes:

- **Multi-Instance**: Monitor multiple Aspire apps simultaneously
- **Advanced Metrics**: Historical trends and threshold-based alerts
- **Cross-Platform**: macOS and Linux support
- **Web Dashboard**: Companion web UI for more detailed analysis

---

## Try It Today

AspireMonitor v1.3.0 is production-ready and available on NuGet:

```bash
dotnet tool install --global ElBruno.AspireMonitor
aspiremon
```

**Update to latest:**
```bash
dotnet tool update --global ElBruno.AspireMonitor
```

**Requirements:** Windows 10+, .NET 10 Runtime

Feedback welcome! Found a bug? Have a feature idea? [Open an issue on GitHub](https://github.com/elbruno/ElBruno.AspireMonitor/issues).

---

## About the Author

**Bruno Capuano** ([@elbruno](https://github.com/elbruno)) is a Microsoft AI MVP and GitHub Star based in Barcelona. He builds open-source tools for .NET developers and shares his work via his blog ([elbruno.com](https://elbruno.com)) and YouTube. When not coding, he's probably thinking about how to make developer tools faster and more intuitive.

**Connect:**
- GitHub: [@elbruno](https://github.com/elbruno)
- Blog: [elbruno.com](https://elbruno.com)
- LinkedIn: [/in/elbruno](https://linkedin.com/in/elbruno)
- Twitter: [@elbruno](https://twitter.com/elbruno)
- YouTube: [@elbruno](https://youtube.com/@elbruno)

---

**Happy monitoring!** 🚀
