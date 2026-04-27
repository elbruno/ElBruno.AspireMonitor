## LinkedIn Post: Main Announcement

💻 **Just shipped: AspireMonitor — live visibility into your Aspire distributed apps**

Monitoring Aspire microservices shouldn't mean constant dashboard-switching. I built AspireMonitor—a lightweight Windows system tray app that puts your running resources directly in your taskbar.

**Key features:**
- 🟢 🟡 🔴 Color-coded health status (at a glance)
- 📌 Pin the resources you care about in a mini window
- 🔗 Clickable URLs for instant resource access
- 🪟 Minimal, native Windows integration
- ⚠️ Pinned resource validation—shows warnings when a configured resource is missing

**Get started:**
```bash
dotnet tool install --global ElBruno.AspireMonitor
aspiremon
```

Open source (MIT), zero dependencies, built with .NET 10 + WPF.

Perfect for local development, debugging, and quick status checks.

**GitHub:** github.com/elbruno/ElBruno.AspireMonitor
**NuGet:** nuget.org/packages/ElBruno.AspireMonitor

Feedback welcome! 🚀

#aspire #dotnet #distributed-systems #opensource #monitoring #windows #developers

---

## LinkedIn Post: Installation Variant

⚡ **New tool: AspireMonitor for Aspire**

One-liner to install:
```bash
dotnet tool install --global ElBruno.AspireMonitor
```

Then launch with: `aspiremon`

Get live status of your Aspire resources in your Windows system tray. No dashboards. No browser tabs. Just one icon that tells you everything.

🟢 = healthy
🟡 = partial
🔴 = stopped

Built for developers who want to focus on code, not monitoring dashboards.

MIT licensed, open source, GitHub: elbruno/ElBruno.AspireMonitor

#aspire #dotnet #devtools #opensource

---

## LinkedIn Post: Features Highlight

🎯 **AspireMonitor: Live Aspire monitoring for Windows**

Tired of switching tabs to check if your microservices are running? Here's what I built:

✅ System tray integration — Always visible, never intrusive
✅ Color-coded status — Green/Yellow/Red at a glance
✅ Pinned resources — Focus on the resources that matter to you
✅ Clickable URLs — Open resources directly from the app
✅ Resource validation — Warnings when a pinned resource is missing
✅ Auto-reconnect — Handles connection issues gracefully
✅ Zero dependencies — Lightweight, fast, native Windows

Works with:
- Local Aspire development
- Docker-based Aspire
- Working folder-based discovery

Install: `dotnet tool install --global ElBruno.AspireMonitor`
Launch: `aspiremon`

GitHub: elbruno/ElBruno.AspireMonitor (MIT, contributions welcome!)

#aspire #dotnet #opensource #monitoring #developers

---

## LinkedIn Post: Developer-Focused

🔧 **Built a monitoring tool for Aspire developers**

Problem: Aspire dashboards are awesome, but keeping them visible while you code is friction.

Solution: AspireMonitor—a system tray app that shows live resource status without leaving your editor.

**Architecture:**
- Polling service calling `aspire describe --format json`
- MVVM-clean code for testability
- Pinned resource filtering and validation
- Exponential backoff on connection failures
- WPF for native Windows experience

**Tech:**
- .NET 10, WPF, xUnit + Moq
- OIDC publishing to NuGet
- GitHub Actions CI/CD

**Open to PRs and feedback:** github.com/elbruno/ElBruno.AspireMonitor

#aspire #dotnet #opensource #architecture #windows

---

## LinkedIn Post: Launch Week

📢 **Launching AspireMonitor v1.0 today!**

A lightweight, open-source monitoring tool for Aspire distributed applications.

**What it does:**
- Shows status of all Aspire resources in Windows system tray (🟢/🟡/🔴)
- Pin the resources you care about for quick access
- Clickable resource URLs for instant open
- Validates pinned resources (warns if a configured resource is missing)

**Perfect for:**
- Local microservices development
- Debugging and quick status checks
- Keeping developers informed without context-switching

**Try it:**
```bash
dotnet tool install --global ElBruno.AspireMonitor
aspiremon
```

Built with .NET 10, WPF, and ❤️ for the Aspire community.

**GitHub:** github.com/elbruno/ElBruno.AspireMonitor
**NuGet:** nuget.org/packages/ElBruno.AspireMonitor
**MIT License** — Contributions welcome!

Thanks to everyone who contributed feedback during development. Your input made this tool better.

What feature would you like to see next?

#aspire #dotnet #opensource #launch #developers #monitoring

---

## LinkedIn Post: Call-to-Action

🚀 **AspireMonitor is live!**

Live monitoring for your Aspire apps—no more dashboard-switching.

**Install:** `dotnet tool install --global ElBruno.AspireMonitor`
**Run:** `aspiremon`

Features:
- 🟢🟡🔴 Color-coded status indicators
- 📌 Pin resources you care about
- 🔗 Clickable resource URLs
- ⚠️ Pinned resource validation
- 🪟 Native Windows system tray

**Give it a try and let me know what you think!**

Feedback, feature requests, and contributions welcome.

GitHub: github.com/elbruno/ElBruno.AspireMonitor

#aspire #dotnet #opensource #monitoring

---

## Usage Notes for LinkedIn

Each post above can be:
1. Posted standalone
2. Posted as a series throughout launch week
3. Combined/edited for your personal voice
4. Shared in LinkedIn comments/discussions

**Best practices:**
- Post during business hours (8am-5pm in your timezone)
- Engage with comments in first hour
- Mix technical and casual posts
- Link back to blog post and GitHub
- Include relevant emojis (easier to scan in feed)
- Tag relevant communities: #aspire #dotnet #opensource
