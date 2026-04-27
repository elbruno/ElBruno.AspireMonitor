# Twitter / X Posts — AspireMonitor v1.4.0

### Main Launch

🚀 AspireMonitor v1.4.0 is out — a Windows tray app for your Aspire AppHost.

📌 Pin the resources you care about
🔗 Full URLs in the mini window
▶️ Start / Stop your AppHost from the tray (Start stays disabled with a live countdown until resources show up)

`dotnet tool install --global ElBruno.AspireMonitor`

github.com/elbruno/ElBruno.AspireMonitor

#aspire #dotnet

---

### Mini Window

🪟 Small thing, big quality-of-life win in AspireMonitor v1.4:

Set `MiniWindowResources: "web, store, gateway"` in settings → get a compact panel pinned to your screen with exactly those three resources and their live URLs.

Prefix match, case-insensitive. Aspire replica suffixes (`web-xggqzmyn`) just work.

github.com/elbruno/ElBruno.AspireMonitor

#aspire #dotnet

---

### Start / Stop Fix

🛠 v1.4 fixed the two things that bugged me most about the previous AspireMonitor build:

▶️ Start used to re-enable ~60s before resources actually appeared. Now: stays disabled with `⏳ Starting Aspire... (12 / 90s)`.
⏹ Stop didn't stop. Now it does. And disables itself during shutdown.

Small UX polish. Big difference.

github.com/elbruno/ElBruno.AspireMonitor

#aspire #dotnet

---

### Real URLs

🔗 Tiny but satisfying v1.4 change in AspireMonitor:

Pinned resources used to render as a generic "🔗 Open" link.
Useless when you have three pinned web services.

Now you see the actual URL inline (`http://localhost:5021`).

You can tell them apart. Crazy concept.

github.com/elbruno/ElBruno.AspireMonitor

#aspire #dotnet

---

### Settings

⚙️ AspireMonitor's entire config surface fits on a Tweet:

```json
{
  "WorkingFolder": "...AppHost",
  "AspireHostUrl": "http://localhost:18888",
  "PollingIntervalMs": 2000,
  "MiniWindowResources": "web, store, gateway"
}
```

Four fields. No wizard. Live reload.

`dotnet tool install --global ElBruno.AspireMonitor`

#aspire #dotnet

---

### One-liner Install

⚡ One command to monitor your Aspire AppHost from the Windows tray:

`dotnet tool install --global ElBruno.AspireMonitor`

Then: `aspiremon`

github.com/elbruno/ElBruno.AspireMonitor

#aspire #dotnet #opensource

---

### Architecture

📐 AspireMonitor v1.4 stack:

✅ .NET 10 + WPF
✅ MVVM, xUnit + Moq
✅ Polls `aspire describe --format json` (no third-party SDK)
✅ Distributed as a .NET global tool
✅ NuGet trusted publishing (OIDC, no long-lived keys)

Open source, MIT.

github.com/elbruno/ElBruno.AspireMonitor

#aspire #dotnet #opensource

---

### Engagement

🗳 Aspire devs — what's the most useful thing I could add to AspireMonitor next?

🔁 Multi-AppHost switcher
🔔 Toast notifications when pins go down
🍎 Cross-platform tray (Avalonia)
📜 Per-resource log viewer

Reply or vote 👇 — roadmap follows what you'd actually use.

github.com/elbruno/ElBruno.AspireMonitor

#aspire #dotnet

---

### Threading Strategy

Suggested order for a launch week:
- Day 1 — Main Launch
- Day 2 — Mini Window
- Day 3 — Start / Stop Fix
- Day 4 — Real URLs
- Day 5 — Settings + Install one-liner
- Week 2 — Architecture, Engagement

> 📸 Add a screenshot to each tweet before posting (mini window + settings dialog get the most engagement).
