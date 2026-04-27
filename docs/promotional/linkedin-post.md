# LinkedIn Posts — AspireMonitor v1.4.0

## Main Announcement

🚀 **AspireMonitor v1.4.0 is out — a Windows tray app for your Aspire AppHost**

If you build distributed apps with Aspire, you've probably wished you could just glance at "what's running, where can I hit it, and is it up" without bouncing to a browser dashboard.

That's what AspireMonitor does. v1.4.0 is the version that finally feels good for daily use:

📌 **Pin the resources you care about** — comma-separated list in settings (`web, store, gateway`) and the mini window shows exactly those, with their real URLs (no more generic "Open" links).

▶️ **Start / Stop your AppHost from the tray** — Start now stays disabled with a live countdown until resources actually appear (no more "wait, why is nothing here?"). Stop now actually stops.

🔗 **Full URLs in pinned resources** — see `http://localhost:5021` instead of guessing what 🔗 Open meant.

🛡 **Pinned-resource validation** — pin a resource that isn't there and the mini window still opens; missing pins are skipped, not crashes.

Install:
```bash
dotnet tool install --global ElBruno.AspireMonitor
aspiremon
```

Open source (MIT), built with .NET 10 + WPF, no third-party Aspire SDK dependency — just shells out to `aspire describe`.

GitHub: github.com/elbruno/ElBruno.AspireMonitor
NuGet: nuget.org/packages/ElBruno.AspireMonitor

Feedback and PRs welcome 🙌

#aspire #dotnet #devtools #opensource #windows

---

## What's New in v1.4.0

🆕 **AspireMonitor v1.4.0 — what changed**

Three things I fixed this release because they were genuinely annoying:

1️⃣ **Start button lied.** It would re-enable the moment Aspire's gateway came up, ~60s before resources were visible. Now it stays disabled with `⏳ Starting Aspire... (12 / 90s)` until things are actually ready.

2️⃣ **Stop button didn't stop.** Now it does, and disables itself while the shutdown is in flight.

3️⃣ **"Open" link was useless.** Pinned resources now show their full URL inline so you can tell three pinned web resources apart at a glance.

Plus a new setting — `MiniWindowResources` — lets you pin a comma-separated list of resource name prefixes to a compact mini window. Case-insensitive, so `web` matches `web-xggqzmyn` (Aspire replica suffixing).

`dotnet tool update --global ElBruno.AspireMonitor`

GitHub: github.com/elbruno/ElBruno.AspireMonitor

#aspire #dotnet #opensource

---

## Mini Window Highlight

🪟 **AspireMonitor mini window — the feature I use the most**

Configure once:
```json
{
  "WorkingFolder": "C:\\Projects\\MyApp\\src\\MyApp.AppHost",
  "MiniWindowResources": "web, store, gateway"
}
```

Get a small panel that shows exactly those three resources with their live URLs, plus Start / Stop / dashboard buttons. Sticks to the corner of your screen, auto-resizes, never gets in the way.

Behind the scenes: prefix match against the resources `aspire describe` returns, ordered the way you typed them. Replicas (`web-xggqzmyn`) match `web`. Resources without an endpoint render as plain text (databases, queues) so you still see they're configured.

Try it: `dotnet tool install --global ElBruno.AspireMonitor` then `aspiremon`.

github.com/elbruno/ElBruno.AspireMonitor

#aspire #dotnet #devtools

---

## Settings Walkthrough

⚙️ **AspireMonitor settings — the whole config**

Just four fields:

🔹 `WorkingFolder` — where your AppHost project lives. AspireMonitor runs `aspire describe` from here.
🔹 `AspireHostUrl` — dashboard URL (defaults to `http://localhost:18888`).
🔹 `PollingIntervalMs` — how often to refresh (default 2000).
🔹 `MiniWindowResources` — comma-separated list of resource name prefixes to pin to the mini window.

Edit from the in-app Settings dialog or `%APPDATA%\Local\ElBruno\AspireMonitor\config.json`. Changes apply live — no restart.

That's the whole product surface. Small on purpose.

`dotnet tool install --global ElBruno.AspireMonitor`

github.com/elbruno/ElBruno.AspireMonitor

#aspire #dotnet #opensource

---

## Developer / Architecture Post

🔧 **What's inside AspireMonitor**

Open-sourced architecture for anyone curious how a small Windows-only Aspire tool is wired:

🟢 **`AspireCliService`** — shells out to `aspire describe --format json`, parses resources (name, type, status, URLs)
🟢 **`AspirePollingService`** — interval-based refresh with cancellation
🟢 **`MainViewModel`** — holds resources, owns Start/Stop commands, exposes config to child VMs
🟢 **`MiniMonitorViewModel`** — prefix-matches `MiniWindowResources` against the resource list
🟢 **WPF + MVVM** — testable VMs, thin views
🟢 **xUnit + Moq** — covering parser, polling, and pinned-resource matching
🟢 **GitHub Actions + NuGet trusted publishing (OIDC)** — no long-lived API keys

No third-party Aspire SDK dependency. No agents. No daemons. Just a tray app talking to the Aspire CLI.

PRs welcome: github.com/elbruno/ElBruno.AspireMonitor

#aspire #dotnet #opensource #architecture

---

## Call to Action

🎯 **AspireMonitor v1.4.0 is live**

If you're an Aspire developer on Windows, give it 30 seconds:

```bash
dotnet tool install --global ElBruno.AspireMonitor
aspiremon
```

Point it at your AppHost folder, pin two or three resources, and tell me what you'd want next.

I want to know:
- Multi-AppHost support — would you use it?
- Toast notifications when a pinned resource goes down — useful or noisy?
- Cross-platform (Avalonia) — what % of you are on macOS/Linux?

Drop a comment 👇 — roadmap is driven by what you actually use.

github.com/elbruno/ElBruno.AspireMonitor

#aspire #dotnet #opensource #devtools

---

## Usage Notes

Each post above is standalone. Suggested order for a launch week:
1. Day 1 morning — Main Announcement
2. Day 1 evening — What's New
3. Day 2 — Mini Window Highlight
4. Day 3 — Settings Walkthrough
5. Day 4 — Developer / Architecture
6. End of week — Call to Action

> 📸 **Add screenshots before publishing** — main window, mini window, and Settings dialog. LinkedIn posts with images get materially more reach than text-only.
