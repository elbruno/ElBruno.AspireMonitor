# Blog Post: Introducing AspireMonitor

## Title

**Introducing AspireMonitor: Real-Time Aspire Monitoring in Your System Tray**

---

## Headline

For developers building distributed applications with Aspire, staying aware of your services is critical—but checking dashboards constantly breaks focus. **AspireMonitor** puts live Aspire resource status directly in your Windows system tray, giving you instant visibility into what's running without leaving your code editor.

---

## Why This Matters

Aspire has revolutionized distributed application development by giving developers a unified, intuitive way to orchestrate microservices, databases, and caches locally. But Aspire's excellent dashboard lives in a browser tab—one more thing to context-switch to when you need to check service status or reachability.

**AspireMonitor solves this problem.** A lightweight Windows system tray icon gives you live status at a glance:
- 🟢 **Green**: All resources running
- 🟡 **Yellow**: Some resources unavailable or partial
- 🔴 **Red**: No resources running

---

## Key Features

### ⚡ Live Status Updates

AspireMonitor checks your running Aspire resources and updates instantly. No manual refreshes. No lag. Just live data.

### 🚦 Color-Coded Status

At a glance, know if your Aspire infrastructure is running:
- Green icon? All resources running.
- Yellow icon? Some resources down or unreachable.
- Red icon? No resources running.

### 🔗 Clickable URLs

Right-click any resource in the expanded view and open it directly—no copy/paste, no hunting through logs.

### 📌 Pin Your Resources

Focus on the resources that matter. Configure a comma-separated list of resources to pin in your mini window, and AspireMonitor highlights just those with live status and clickable URLs:

```json
{
  "workingFolder": "C:\\Projects\\MyAspireApp",
  "MiniWindowResources": "api,frontend,db"
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

Edit `%APPDATA%\Local\ElBruno\AspireMonitor\config.json` to set your working folder and pin resources. Restart the app and you're done.

---

## Perfect For

- **Local Development**: Keep tabs on your microservices without leaving your editor
- **Debugging**: Quickly check which resources are running or down
- **Teaching**: Show students your Aspire setup at a glance
- **Quick Checks**: Verify all services are up before running tests or committing code

---

## How It Works (Architecture)

AspireMonitor uses a simple, elegant architecture:

1. **AspireCliClient** — Calls `aspire describe --format json` against your Aspire AppHost
2. **PollingService** — Runs background checks at configurable interval
3. **ResourceStatusEvaluator** — Determines resource health (running/partial/stopped)
4. **MainViewModel** — MVVM binding to WPF
5. **WPF UI** — Lightweight notification window + system tray + mini window with pinned resources

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
- **Cross-Platform**: macOS and Linux support (WPF → WinUI/Avalonia)
- **Web Companion**: Companion dashboard for remote monitoring
- **Custom Views**: More flexible resource filtering and grouping

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
